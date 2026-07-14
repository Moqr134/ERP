using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.StockTransactionsEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.StockTransactionDto;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class StockTransactionsService : MasterService, IScopped, IStockTransactionsService
    {
        public StockTransactionsService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<List<StockTransactionDto>> GetStockTransactionsAsync()
        {
            List<StockTransactionDto> list = await (
                from s in _context.StockTransactions
                join p in _context.Products.IgnoreQueryFilters() on s.ProductId equals p.Id into productJoin
                from p in productJoin.DefaultIfEmpty()
                orderby s.CreateDate descending
                select new StockTransactionDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    ProductName = p != null ? p.Name : "المنتج محذوف",
                    Quantity = s.Quantity,
                    TransactionType = s.TransactionType,
                    ReferenceId = s.ReferenceId,
                    Notes = s.Notes,
                    CreateDate = s.CreateDate,
                    CreateUserId = s.CreateUserId
                })
                .Take(500)
                .ToListAsync();
            return list;
        }
        private async Task<Product?> GetProductAsync(int Id)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == Id && !p.IsRemoved);
            return product;
        }
        public async Task AddStockTransaction(CreateStockTransactionsModel Model, int userId)
        {
            if (Model.Quantity <= 0)
                throw new InvalidOperationException("الكمية يجب أن تكون أكبر من صفر");

            var transactionType = (Model.TransactionType ?? string.Empty).Trim();
            if (transactionType is not ("In" or "Out"))
                throw new InvalidOperationException("نوع الفاتورة غير صحيح");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Product? product = await GetProductAsync(Model.ProductId);
                if (product == null) throw new KeyNotFoundException("لم يتم العثور على المنتج");

                if (transactionType == "Out")
                {
                    if (Model.Quantity > product.CurrentStock)
                        throw new InvalidOperationException("الكمية الموجودة في الخزن اقل من الكمية الصادرة");
                    product.CurrentStock -= Model.Quantity;
                }
                else
                {
                    product.CurrentStock += Model.Quantity;
                }

                StockTransactions stockTransactions = _mapper.Map<StockTransactions>(Model);
                stockTransactions.TransactionType = transactionType;
                stockTransactions.CreateDate = DateTime.UtcNow.AddHours(3);
                stockTransactions.CreateUserId = userId;
                _context.Products.Entry(product).State = EntityState.Modified;
                _context.StockTransactions.Add(stockTransactions);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
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
