using AutoMapper;
using ERP_API.App.IService;
using ERP_API.App.Inventory;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.SalesEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Infrastructure.Money;
using ERP_API.Infrastructure.Services;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.SalesDto;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class SalesService : MasterService, IScopped, ISalesService
    {
        private readonly IProductService _productService;

        public SalesService(DBContext context, IMapper mapper, IProductService productService)
            : base(context, mapper)
        {
            _productService = productService;
        }

        public async Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode, int? warehouseId = null)
            => await _productService.LookupByBarcodeAsync(barcode, warehouseId);

        public async Task<List<ProductDto>> SearchProductsAsync(string term, int take = 12, int? warehouseId = null)
        {
            if (string.IsNullOrWhiteSpace(term))
                return [];

            take = take is < 1 ? 12 : take > 30 ? 30 : take;
            var q = term.Trim();

            var products = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsRemoved && (
                    p.Name.Contains(q)
                    || p.Barcode.Contains(q)
                    || p.SKU.Contains(q)
                    || p.Barcodes.Any(b => !b.IsRemoved && b.Barcode.Contains(q))))
                .OrderBy(p => p.Name)
                .Take(take)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    SKU = p.SKU,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    CurrentStock = p.CurrentStock,
                    MinStockLevel = p.MinStockLevel,
                    CategoriesId = p.CategoriesId,
                    WarehouseId = p.WarehouseId
                })
                .ToListAsync();

            var ids = products.Select(p => p.Id).ToList();
            if (ids.Count == 0)
                return products;

            if (warehouseId is > 0)
            {
                var balances = await _context.WarehouseStocks
                    .AsNoTracking()
                    .Where(s => ids.Contains(s.ProductId) && s.WarehouseId == warehouseId.Value && !s.IsRemoved)
                    .ToDictionaryAsync(s => s.ProductId, s => s.Quantity);
                foreach (var p in products)
                    p.CurrentStock = balances.TryGetValue(p.Id, out var qty) ? qty : 0;
            }

            var units = await _context.ProductUnits
                .AsNoTracking()
                .Where(u => ids.Contains(u.ProductId) && !u.IsRemoved)
                .OrderBy(u => u.SortOrder)
                .Select(u => new
                {
                    u.ProductId,
                    Unit = new ProductUnitDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Factor = u.Factor,
                        SellingPrice = u.SellingPrice,
                        IsBase = u.IsBase,
                        IsDefaultForSale = u.IsDefaultForSale,
                        SortOrder = u.SortOrder,
                        Barcodes = u.Barcodes.Where(b => !b.IsRemoved)
                            .OrderByDescending(b => b.IsPrimary)
                            .Select(b => new ProductBarcodeDto
                            {
                                Id = b.Id,
                                Barcode = b.Barcode,
                                IsPrimary = b.IsPrimary
                            }).ToList()
                    }
                })
                .ToListAsync();

            var byProduct = units.GroupBy(x => x.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Unit).ToList());

            foreach (var p in products)
                p.Units = byProduct.TryGetValue(p.Id, out var list) ? list : new();

            return products;
        }

        public async Task<SaleDto> CompleteSaleAsync(CompleteSaleModel model, int userId)
        {
            if (model.Lines is null || model.Lines.Count == 0)
                throw new InvalidOperationException("يجب إضافة منتج واحد على الأقل");

            var paymentMethod = (model.PaymentMethod ?? string.Empty).Trim();
            if (paymentMethod is not ("Cash" or "Card"))
                throw new InvalidOperationException("طريقة الدفع غير صحيحة");

            // Merge by product + packaging unit
            var mergedLines = model.Lines
                .GroupBy(l => new { l.ProductId, UnitId = l.ProductUnitId ?? 0 })
                .Select(g => new CompleteSaleLineDto
                {
                    ProductId = g.Key.ProductId,
                    ProductUnitId = g.Key.UnitId == 0 ? null : g.Key.UnitId,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.LastOrDefault(x => x.UnitPrice is > 0)?.UnitPrice,
                    Barcode = g.Select(x => x.Barcode).FirstOrDefault(b => !string.IsNullOrWhiteSpace(b))
                })
                .ToList();

            if (mergedLines.Any(l => l.Quantity <= 0))
                throw new InvalidOperationException("كمية المنتج يجب أن تكون أكبر من صفر");

            if (model.Discount < 0)
                throw new InvalidOperationException("الخصم لا يمكن أن يكون سالباً");

            if (model.WarehouseId <= 0)
                throw new InvalidOperationException("يجب اختيار مخزن البيع");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await WarehouseStockHelper.EnsureWarehouseActiveAsync(_context, model.WarehouseId);

                var productIds = mergedLines.Select(l => l.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Include(p => p.Units.Where(u => !u.IsRemoved))
                        .ThenInclude(u => u.Barcodes.Where(b => !b.IsRemoved))
                    .Where(p => productIds.Contains(p.Id) && !p.IsRemoved)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    throw new KeyNotFoundException("أحد المنتجات غير موجود");

                var productMap = products.ToDictionary(p => p.Id);
                var now = DateTime.UtcNow.AddHours(3);
                var invoiceNumber = await GenerateInvoiceNumberAsync(now);

                // Aggregate base-unit deductions per product for a single stock check
                var baseNeeded = new Dictionary<int, int>();
                var resolved = new List<(CompleteSaleLineDto Line, Product Product, ProductUnit Unit, int BaseQty, double UnitPrice, double LineTotal)>();

                foreach (var line in mergedLines)
                {
                    var product = productMap[line.ProductId];
                    var unit = ResolveUnit(product, line.ProductUnitId);

                    var baseQty = checked(line.Quantity * unit.Factor);
                    baseNeeded[product.Id] = baseNeeded.GetValueOrDefault(product.Id) + baseQty;

                    var unitPrice = line.UnitPrice is > 0 ? line.UnitPrice.Value : unit.SellingPrice;
                    if (unitPrice < 0)
                        throw new InvalidOperationException("سعر البيع غير صحيح");

                    var lineTotal = Math.Round(unitPrice * line.Quantity, MidpointRounding.AwayFromZero);
                    resolved.Add((line, product, unit, baseQty, unitPrice, lineTotal));
                }

                foreach (var (productId, needed) in baseNeeded)
                {
                    var product = productMap[productId];
                    var available = await WarehouseStockHelper.GetQuantityAsync(_context, productId, model.WarehouseId);
                    if (needed > available)
                        throw new InvalidOperationException($"المخزون غير كافٍ في المخزن للمنتج: {product.Name}");
                }

                var saleLines = new List<SaleLine>();
                double subTotal = 0;

                foreach (var item in resolved)
                {
                    subTotal += item.LineTotal;
                    await WarehouseStockHelper.ApplyDeltaAsync(
                        _context, item.Product, model.WarehouseId, -item.BaseQty, userId, now, item.Product.Name);

                    var barcode = !string.IsNullOrWhiteSpace(item.Line.Barcode)
                        ? item.Line.Barcode
                        : item.Unit.Barcodes.FirstOrDefault(b => b.IsPrimary)?.Barcode
                          ?? item.Product.Barcode;

                    saleLines.Add(new SaleLine
                    {
                        ProductId = item.Product.Id,
                        ProductName = item.Product.Name,
                        Barcode = barcode,
                        Quantity = item.Line.Quantity,
                        BaseQuantity = item.BaseQty,
                        UnitName = item.Unit.Name,
                        UnitFactor = item.Unit.Factor,
                        ProductUnitId = item.Unit.Id > 0 ? item.Unit.Id : null,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal,
                        CreateDate = now,
                        CreateUserId = userId
                    });

                    _context.StockTransactions.Add(new StockTransactions
                    {
                        ProductId = item.Product.Id,
                        WarehouseId = model.WarehouseId,
                        Quantity = item.BaseQty,
                        TransactionType = "Out",
                        ReferenceId = invoiceNumber,
                        Notes = $"بيع مباشر POS ({item.Unit.Name})",
                        CreateDate = now,
                        CreateUserId = userId
                    });
                }

                subTotal = Math.Round(subTotal, MidpointRounding.AwayFromZero);
                var discount = Math.Round(model.Discount, MidpointRounding.AwayFromZero);
                if (discount > subTotal)
                    throw new InvalidOperationException("الخصم أكبر من مجموع الفاتورة");

                var total = Math.Round(subTotal - discount, MidpointRounding.AwayFromZero);
                var paidAmount = Math.Round(model.PaidAmount, MidpointRounding.AwayFromZero);

                if (paymentMethod == "Cash")
                {
                    if (paidAmount < total)
                        throw new InvalidOperationException("المبلغ المدفوع أقل من إجمالي الفاتورة");
                }
                else
                {
                    paidAmount = total;
                }

                var changeAmount = IraqiCurrency.CalculateChange(total, paidAmount);

                var sale = new Sale
                {
                    InvoiceNumber = invoiceNumber,
                    PaymentMethod = paymentMethod,
                    SubTotal = subTotal,
                    Discount = discount,
                    Total = total,
                    PaidAmount = paidAmount,
                    ChangeAmount = changeAmount,
                    Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                    Status = "Completed",
                    WarehouseId = model.WarehouseId,
                    CreateDate = now,
                    CreateUserId = userId,
                    Lines = saleLines
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapSale(sale);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("تم تعديل المخزون من عملية أخرى، أعد المحاولة");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SalesListResponse> GetSalesAsync(PageDto page)
        {
            page ??= new PageDto();
            var query = _context.Sales.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(page.SearchTerm))
            {
                var term = page.SearchTerm.Trim();
                query = query.Where(s =>
                    s.InvoiceNumber.Contains(term)
                    || (s.Notes != null && s.Notes.Contains(term))
                    || s.PaymentMethod.Contains(term));
            }

            var totalCount = await query.CountAsync();
            var pageSize = page.PageSize;
            var pageIndex = page.PageIndex;
            var pageCount = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(s => s.CreateDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(s => s.Lines)
                .Include(s => s.Warehouse)
                .ToListAsync();

            return new SalesListResponse
            {
                Items = items.Select(MapSale).ToList(),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                PageCount = pageCount
            };
        }

        public async Task<SaleDto> GetSaleByIdAsync(int id)
        {
            var sale = await _context.Sales
                .AsNoTracking()
                .Include(s => s.Lines)
                .Include(s => s.Warehouse)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                throw new KeyNotFoundException("لم يتم العثور على الفاتورة");

            return MapSale(sale);
        }

        private static ProductUnit ResolveUnit(Product product, int? productUnitId)
        {
            if (productUnitId is > 0)
            {
                var unit = product.Units.FirstOrDefault(u => u.Id == productUnitId.Value && !u.IsRemoved);
                if (unit is null)
                    throw new KeyNotFoundException("وحدة البيع غير موجودة للمنتج");
                return unit;
            }

            return product.Units.FirstOrDefault(u => u.IsDefaultForSale && !u.IsRemoved)
                ?? product.Units.FirstOrDefault(u => u.IsBase && !u.IsRemoved)
                ?? product.Units.FirstOrDefault(u => !u.IsRemoved)
                ?? new ProductUnit
                {
                    Id = 0,
                    Name = "مفرد",
                    Factor = 1,
                    SellingPrice = product.SellingPrice,
                    IsBase = true,
                    IsDefaultForSale = true
                };
        }

        private async Task<string> GenerateInvoiceNumberAsync(DateTime now)
        {
            var prefix = $"POS-{now:yyyyMMdd}-";
            var last = await _context.Sales
                .IgnoreQueryFilters()
                .Where(s => s.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(s => s.InvoiceNumber)
                .Select(s => s.InvoiceNumber)
                .FirstOrDefaultAsync();

            var seq = 1;
            if (!string.IsNullOrEmpty(last) && last.Length > prefix.Length
                && int.TryParse(last[prefix.Length..], out var parsed))
            {
                seq = parsed + 1;
            }

            return $"{prefix}{seq:D4}";
        }

        private static SaleDto MapSale(Sale sale) => new()
        {
            Id = sale.Id,
            InvoiceNumber = sale.InvoiceNumber,
            PaymentMethod = sale.PaymentMethod,
            SubTotal = sale.SubTotal,
            Discount = sale.Discount,
            Total = sale.Total,
            PaidAmount = sale.PaidAmount,
            ChangeAmount = sale.ChangeAmount,
            Notes = sale.Notes,
            Status = sale.Status,
            WarehouseId = sale.WarehouseId,
            WarehouseName = sale.Warehouse?.Name,
            CreateDate = sale.CreateDate,
            CreateUserId = sale.CreateUserId,
            Lines = sale.Lines?
                .OrderBy(l => l.Id)
                .Select(l => new SaleLineDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductName = l.ProductName,
                    Barcode = l.Barcode,
                    Quantity = l.Quantity,
                    BaseQuantity = l.BaseQuantity > 0 ? l.BaseQuantity : l.Quantity,
                    UnitName = string.IsNullOrWhiteSpace(l.UnitName) ? "مفرد" : l.UnitName,
                    UnitFactor = l.UnitFactor <= 0 ? 1 : l.UnitFactor,
                    ProductUnitId = l.ProductUnitId,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal
                })
                .ToList() ?? []
        };
    }
}
