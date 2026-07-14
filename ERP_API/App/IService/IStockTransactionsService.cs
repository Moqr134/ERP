using ERPDto.StockTransactionDto;

namespace ERP_API.App.IService
{
    public interface IStockTransactionsService
    {
        public Task AddStockTransaction(CreateStockTransactionsModel Model,int userId);
        public Task<List<StockTransactionDto>> GetStockTransactionsAsync();
    }
}
