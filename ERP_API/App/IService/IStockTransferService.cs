using ERPDto.StockTransferDto;

namespace ERP_API.App.IService
{
    public interface IStockTransferService
    {
        Task<StockTransferDto> CreateTransferAsync(CreateStockTransferModel model, int userId);
        Task<List<StockTransferDto>> GetTransfersAsync();
        Task<StockTransferDto> GetTransferByIdAsync(int id);
    }
}
