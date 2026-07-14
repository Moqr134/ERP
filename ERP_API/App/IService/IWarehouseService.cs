using ERPDto.WarehouseDto;

namespace ERP_API.App.IService
{
    public interface IWarehouseService
    {
        Task<List<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto> GetWarehouseByIdAsync(int id);
        Task AddWarehouseAsync(WarehouseModel model, int createUserId);
        Task EditWarehouseAsync(WarehouseModel model, int updateUserId);
        Task DeleteWarehouseAsync(int id, int deleteUserId);
    }
}
