using ERPDto.StockTransactionDto;

namespace ERP_API.App.IService
{
    public interface IStockTransactionsService
    {
        Task AddStockTransaction(CreateStockTransactionsModel model, int userId);
        Task<List<StockTransactionDto>> GetStockTransactionsAsync(int? warehouseId = null);
    }
}
