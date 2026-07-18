using AutoMapper;
using ERP_API.App.IService;
using ERP_API.App.Inventory;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.PurchaseEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.PurchaseDto;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class PurchaseReceiptService : MasterService, IScopped, IPurchaseReceiptService
    {
        private readonly IProductService _productService;

        public PurchaseReceiptService(DBContext context, IMapper mapper, IProductService productService)
            : base(context, mapper)
        {
            _productService = productService;
        }

        public async Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode, int? warehouseId = null)
            => await _productService.LookupByBarcodeAsync(barcode, warehouseId);

        public async Task<List<ProductDto>> SearchProductsAsync(string term, int take = 12, int? warehouseId = null)
            => await _productService.SearchProductsAsync(term, take, warehouseId);

        public async Task<PurchaseReceiptDto> CompleteReceiptAsync(CompletePurchaseReceiptModel model, int userId)
        {
            if (model.Lines is null || model.Lines.Count == 0)
                throw new InvalidOperationException("يجب إضافة منتج واحد على الأقل");

            if (model.SupplierId <= 0)
                throw new InvalidOperationException("يجب اختيار المورد");

            if (model.WarehouseId <= 0)
                throw new InvalidOperationException("يجب اختيار مخزن الاستلام");

            if (model.Discount < 0)
                throw new InvalidOperationException("الخصم لا يمكن أن يكون سالباً");

            var mergedLines = model.Lines
                .GroupBy(l => new { l.ProductId, UnitId = l.ProductUnitId ?? 0 })
                .Select(g =>
                {
                    var lastCost = g.LastOrDefault(x => x.UnitCost is > 0)?.UnitCost;
                    return new CompletePurchaseLineDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductUnitId = g.Key.UnitId == 0 ? null : g.Key.UnitId,
                        Quantity = g.Sum(x => x.Quantity),
                        UnitCost = lastCost,
                        Barcode = g.Select(x => x.Barcode).FirstOrDefault(b => !string.IsNullOrWhiteSpace(b))
                    };
                })
                .ToList();

            if (mergedLines.Any(l => l.Quantity <= 0))
                throw new InvalidOperationException("كمية المنتج يجب أن تكون أكبر من صفر");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await WarehouseStockHelper.EnsureWarehouseActiveAsync(_context, model.WarehouseId);

                var supplierExists = await _context.Suppliers
                    .AnyAsync(s => s.Id == model.SupplierId && !s.IsRemoved);
                if (!supplierExists)
                    throw new KeyNotFoundException("المورد غير موجود");

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
                var receiptNumber = await GenerateReceiptNumberAsync(now);

                var receiptLines = new List<PurchaseReceiptLine>();
                double subTotal = 0;

                foreach (var line in mergedLines)
                {
                    var product = productMap[line.ProductId];
                    var unit = ResolveUnit(product, line.ProductUnitId);
                    var baseQty = checked(line.Quantity * unit.Factor);

                    var unitCost = line.UnitCost.HasValue && line.UnitCost.Value >= 0
                        ? line.UnitCost.Value
                        : Math.Round(product.CostPrice * unit.Factor, MidpointRounding.AwayFromZero);

                    if (unitCost < 0)
                        throw new InvalidOperationException("كلفة الوحدة غير صحيحة");

                    var lineTotal = Math.Round(unitCost * line.Quantity, MidpointRounding.AwayFromZero);
                    subTotal += lineTotal;

                    await WarehouseStockHelper.ApplyDeltaAsync(
                        _context, product, model.WarehouseId, baseQty, userId, now, product.Name);

                    // Update last purchase cost in base units
                    var baseUnitCost = unit.Factor > 0
                        ? Math.Round(unitCost / unit.Factor, MidpointRounding.AwayFromZero)
                        : unitCost;
                    product.CostPrice = baseUnitCost;
                    product.UpdateDate = now;
                    product.UpdateUserId = userId;

                    var barcode = !string.IsNullOrWhiteSpace(line.Barcode)
                        ? line.Barcode
                        : unit.Barcodes.FirstOrDefault(b => b.IsPrimary)?.Barcode
                          ?? product.Barcode;

                    receiptLines.Add(new PurchaseReceiptLine
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Barcode = barcode,
                        Quantity = line.Quantity,
                        BaseQuantity = baseQty,
                        UnitName = unit.Name,
                        UnitFactor = unit.Factor,
                        ProductUnitId = unit.Id > 0 ? unit.Id : null,
                        UnitCost = unitCost,
                        LineTotal = lineTotal,
                        CreateDate = now,
                        CreateUserId = userId
                    });

                    _context.StockTransactions.Add(new StockTransactions
                    {
                        ProductId = product.Id,
                        WarehouseId = model.WarehouseId,
                        Quantity = baseQty,
                        TransactionType = "In",
                        ReferenceId = receiptNumber,
                        Notes = $"استلام من مورد ({unit.Name})",
                        CreateDate = now,
                        CreateUserId = userId
                    });
                }

                subTotal = Math.Round(subTotal, MidpointRounding.AwayFromZero);
                var discount = Math.Round(model.Discount, MidpointRounding.AwayFromZero);
                if (discount > subTotal)
                    throw new InvalidOperationException("الخصم أكبر من مجموع الفاتورة");

                var total = Math.Round(subTotal - discount, MidpointRounding.AwayFromZero);

                var receipt = new PurchaseReceipt
                {
                    ReceiptNumber = receiptNumber,
                    SupplierId = model.SupplierId,
                    SupplierInvoiceNumber = string.IsNullOrWhiteSpace(model.SupplierInvoiceNumber)
                        ? null
                        : model.SupplierInvoiceNumber.Trim(),
                    SubTotal = subTotal,
                    Discount = discount,
                    Total = total,
                    Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                    Status = "Completed",
                    WarehouseId = model.WarehouseId,
                    CreateDate = now,
                    CreateUserId = userId,
                    Lines = receiptLines
                };

                _context.PurchaseReceipts.Add(receipt);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _productService.InvalidateProductCache();

                // Load names for response
                receipt.Supplier = await _context.Suppliers.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == receipt.SupplierId);
                receipt.Warehouse = await _context.Warehouses.AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == receipt.WarehouseId);

                return MapReceipt(receipt);
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

        public async Task<PurchaseReceiptListResponse> GetReceiptsAsync(PageDto page)
        {
            page ??= new PageDto();
            if (page.PageIndex < 1) page.PageIndex = 1;
            if (page.PageSize < 1) page.PageSize = 10;
            if (page.PageSize > 100) page.PageSize = 100;

            var query = _context.PurchaseReceipts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(page.SearchTerm))
            {
                var term = page.SearchTerm.Trim();
                query = query.Where(r =>
                    r.ReceiptNumber.Contains(term)
                    || (r.SupplierInvoiceNumber != null && r.SupplierInvoiceNumber.Contains(term))
                    || (r.Notes != null && r.Notes.Contains(term))
                    || (r.Supplier != null && r.Supplier.CompanyName.Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var pageCount = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)page.PageSize);

            var items = await query
                .OrderByDescending(r => r.CreateDate)
                .Skip((page.PageIndex - 1) * page.PageSize)
                .Take(page.PageSize)
                .Include(r => r.Lines)
                .Include(r => r.Warehouse)
                .Include(r => r.Supplier)
                .ToListAsync();

            return new PurchaseReceiptListResponse
            {
                Items = items.Select(MapReceipt).ToList(),
                TotalCount = totalCount,
                PageIndex = page.PageIndex,
                PageSize = page.PageSize,
                PageCount = pageCount
            };
        }

        public async Task<PurchaseReceiptDto> GetReceiptByIdAsync(int id)
        {
            var receipt = await _context.PurchaseReceipts
                .AsNoTracking()
                .Include(r => r.Lines)
                .Include(r => r.Warehouse)
                .Include(r => r.Supplier)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt is null)
                throw new KeyNotFoundException("لم يتم العثور على سند الاستلام");

            return MapReceipt(receipt);
        }

        private static ProductUnit ResolveUnit(Product product, int? productUnitId)
        {
            if (productUnitId is > 0)
            {
                var unit = product.Units.FirstOrDefault(u => u.Id == productUnitId.Value && !u.IsRemoved);
                if (unit is null)
                    throw new KeyNotFoundException("وحدة المنتج غير موجودة");
                return unit;
            }

            return product.Units.FirstOrDefault(u => u.IsBase && !u.IsRemoved)
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

        private async Task<string> GenerateReceiptNumberAsync(DateTime now)
        {
            var prefix = $"PUR-{now:yyyyMMdd}-";
            var last = await _context.PurchaseReceipts
                .IgnoreQueryFilters()
                .Where(r => r.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(r => r.ReceiptNumber)
                .Select(r => r.ReceiptNumber)
                .FirstOrDefaultAsync();

            var seq = 1;
            if (!string.IsNullOrEmpty(last) && last.Length > prefix.Length
                && int.TryParse(last[prefix.Length..], out var parsed))
            {
                seq = parsed + 1;
            }

            return $"{prefix}{seq:D4}";
        }

        private static PurchaseReceiptDto MapReceipt(PurchaseReceipt receipt) => new()
        {
            Id = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.Supplier?.CompanyName,
            SupplierInvoiceNumber = receipt.SupplierInvoiceNumber,
            SubTotal = receipt.SubTotal,
            Discount = receipt.Discount,
            Total = receipt.Total,
            Notes = receipt.Notes,
            Status = receipt.Status,
            WarehouseId = receipt.WarehouseId,
            WarehouseName = receipt.Warehouse?.Name,
            CreateDate = receipt.CreateDate,
            CreateUserId = receipt.CreateUserId,
            Lines = receipt.Lines?
                .OrderBy(l => l.Id)
                .Select(l => new PurchaseReceiptLineDto
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    ProductName = l.ProductName,
                    Barcode = l.Barcode,
                    Quantity = l.Quantity,
                    BaseQuantity = l.BaseQuantity,
                    UnitName = l.UnitName,
                    UnitFactor = l.UnitFactor,
                    ProductUnitId = l.ProductUnitId,
                    UnitCost = l.UnitCost,
                    LineTotal = l.LineTotal
                })
                .ToList() ?? new()
        };
    }
}
