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
        private async Task<Product?> GetProductAsync(int Id)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == Id && !p.IsRemoved);
            return product;
        }
        public async Task AddStockTransaction(CreateStockTransactionsModel Model, int userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Product? product = await GetProductAsync(Model.ProductId);
                if (product == null) throw new KeyNotFoundException("لم يتم العثور على المنتج");
                if (Model.TransactionType == "Out")
                {
                    if (Model.Quantity > product.CurrentStock)
                        throw new InvalidOperationException("الكمية الموجودة في الخزن اقل من الكمية الصادرة");
                    product.CurrentStock -= Model.Quantity;
                }
                else if (Model.TransactionType == "In")
                    product.CurrentStock += Model.Quantity;
                else throw new InvalidOperationException("نوع الفاتورة غير صحيح");

                StockTransactions stockTransactions = _mapper.Map<StockTransactions>(Model);
                stockTransactions.CreateDate = DateTime.UtcNow.AddHours(3);
                stockTransactions.CreateUserId = userId;
                _context.Products.Entry(product).State = EntityState.Modified;
                _context.StockTransactions.Add(stockTransactions);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
