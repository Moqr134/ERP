using AutoMapper;
using ERP_API.App.IService;
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
        public SalesService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<ProductDto?> LookupProductByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            var term = barcode.Trim();
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Barcode == term && !p.IsRemoved);

            return product is null ? null : MapProduct(product);
        }

        public async Task<List<ProductDto>> SearchProductsAsync(string term, int take = 12)
        {
            if (string.IsNullOrWhiteSpace(term))
                return [];

            take = take is < 1 ? 12 : take > 30 ? 30 : take;
            var q = term.Trim();

            return await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsRemoved && (
                    p.Name.Contains(q)
                    || p.Barcode.Contains(q)
                    || p.SKU.Contains(q)))
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
                    CategoriesId = p.CategoriesId
                })
                .ToListAsync();
        }

        public async Task<SaleDto> CompleteSaleAsync(CompleteSaleModel model, int userId)
        {
            if (model.Lines is null || model.Lines.Count == 0)
                throw new InvalidOperationException("يجب إضافة منتج واحد على الأقل");

            var paymentMethod = (model.PaymentMethod ?? string.Empty).Trim();
            if (paymentMethod is not ("Cash" or "Card"))
                throw new InvalidOperationException("طريقة الدفع غير صحيحة");

            var mergedLines = model.Lines
                .GroupBy(l => l.ProductId)
                .Select(g => new CompleteSaleLineDto
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    UnitPrice = g.LastOrDefault(x => x.UnitPrice is > 0)?.UnitPrice
                })
                .ToList();

            if (mergedLines.Any(l => l.Quantity <= 0))
                throw new InvalidOperationException("كمية المنتج يجب أن تكون أكبر من صفر");

            if (model.Discount < 0)
                throw new InvalidOperationException("الخصم لا يمكن أن يكون سالباً");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var productIds = mergedLines.Select(l => l.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsRemoved)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    throw new KeyNotFoundException("أحد المنتجات غير موجود");

                var productMap = products.ToDictionary(p => p.Id);
                var now = DateTime.UtcNow.AddHours(3);
                var invoiceNumber = await GenerateInvoiceNumberAsync(now);

                var saleLines = new List<SaleLine>();
                double subTotal = 0;

                foreach (var line in mergedLines)
                {
                    var product = productMap[line.ProductId];
                    if (line.Quantity > product.CurrentStock)
                        throw new InvalidOperationException($"المخزون غير كافٍ للمنتج: {product.Name}");

                    var unitPrice = line.UnitPrice is > 0 ? line.UnitPrice.Value : product.SellingPrice;
                    if (unitPrice < 0)
                        throw new InvalidOperationException("سعر البيع غير صحيح");

                    var lineTotal = Math.Round(unitPrice * line.Quantity, MidpointRounding.AwayFromZero);
                    subTotal += lineTotal;

                    product.CurrentStock -= line.Quantity;
                    _context.Products.Entry(product).State = EntityState.Modified;

                    saleLines.Add(new SaleLine
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Barcode = product.Barcode,
                        Quantity = line.Quantity,
                        UnitPrice = unitPrice,
                        LineTotal = lineTotal,
                        CreateDate = now,
                        CreateUserId = userId
                    });

                    _context.StockTransactions.Add(new StockTransactions
                    {
                        ProductId = product.Id,
                        Quantity = line.Quantity,
                        TransactionType = "Out",
                        ReferenceId = invoiceNumber,
                        Notes = "بيع مباشر POS",
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
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale is null)
                throw new KeyNotFoundException("لم يتم العثور على الفاتورة");

            return MapSale(sale);
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

        private static ProductDto MapProduct(Product product) => new()
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Name = product.Name,
            SKU = product.SKU,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            CurrentStock = product.CurrentStock,
            MinStockLevel = product.MinStockLevel,
            CategoriesId = product.CategoriesId
        };

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
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal
                })
                .ToList() ?? []
        };
    }
}
