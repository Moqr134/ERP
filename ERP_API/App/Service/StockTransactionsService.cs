using AutoMapper;
using ERP_API.App.Inventory;
using ERP_API.App.IService;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Domin.WarehouseEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.StockTransactionDto;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class StockTransactionsService : MasterService, IScopped, IStockTransactionsService
    {
        private readonly IProductService _productService;

        public StockTransactionsService(DBContext context, IMapper mapper, IProductService productService)
            : base(context, mapper)
        {
            _productService = productService;
        }

        public async Task<List<StockTransactionDto>> GetStockTransactionsAsync(int? warehouseId = null)
        {
            var query =
                from s in _context.StockTransactions
                join p in _context.Products.IgnoreQueryFilters() on s.ProductId equals p.Id into productJoin
                from p in productJoin.DefaultIfEmpty()
                join w in _context.Warehouses.IgnoreQueryFilters() on s.WarehouseId equals w.Id into whJoin
                from w in whJoin.DefaultIfEmpty()
                orderby s.CreateDate descending
                select new StockTransactionDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = p != null ? p.Name : "المنتج محذوف",
                    WarehouseId = s.WarehouseId,
                    WarehouseName = w != null ? w.Name : "—",
                    RelatedWarehouseId = s.RelatedWarehouseId,
                    Quantity = s.Quantity,
                    TransactionType = s.TransactionType,
                    ReferenceId = s.ReferenceId,
                    Notes = s.Notes,
                    CreateDate = s.CreateDate,
                    CreateUserId = s.CreateUserId
                };

            if (warehouseId is > 0)
                query = query.Where(x => x.WarehouseId == warehouseId.Value);

            return await query.Take(500).ToListAsync();
        }

        private async Task<Product?> GetProductAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsRemoved);
        }

        public async Task AddStockTransaction(CreateStockTransactionsModel model, int userId)
        {
            if (model.Quantity <= 0)
                throw new InvalidOperationException("الكمية يجب أن تكون أكبر من صفر");

            if (model.WarehouseId <= 0)
                throw new InvalidOperationException("يجب اختيار المخزن");

            var transactionType = (model.TransactionType ?? string.Empty).Trim();
            if (transactionType is not ("In" or "Out"))
                throw new InvalidOperationException("نوع الفاتورة غير صحيح");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await WarehouseStockHelper.EnsureWarehouseActiveAsync(_context, model.WarehouseId);

                Product? product = await GetProductAsync(model.ProductId);
                if (product == null) throw new KeyNotFoundException("لم يتم العثور على المنتج");

                var now = DateTime.UtcNow.AddHours(3);
                var delta = transactionType == "In" ? model.Quantity : -model.Quantity;
                await WarehouseStockHelper.ApplyDeltaAsync(
                    _context, product, model.WarehouseId, delta, userId, now, product.Name);

                var stockTransactions = _mapper.Map<StockTransactions>(model);
                stockTransactions.TransactionType = transactionType;
                stockTransactions.WarehouseId = model.WarehouseId;
                stockTransactions.ReferenceId = string.IsNullOrWhiteSpace(model.ReferenceId)
                    ? $"STK-{now:yyyyMMddHHmmss}"
                    : model.ReferenceId.Trim();
                stockTransactions.Notes = model.Notes?.Trim() ?? string.Empty;
                stockTransactions.CreateDate = now;
                stockTransactions.CreateUserId = userId;
                _context.StockTransactions.Add(stockTransactions);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _productService.InvalidateProductCache();
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
    }
}
