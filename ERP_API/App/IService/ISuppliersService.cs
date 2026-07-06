using ERPDto.Suppliers;

namespace ERP_API.App.IService
{
    public interface ISuppliersService
    {
        public Task<List<SuppliersDto>> GetAllSupplires();
        public Task<SuppliersDto> GetSuppliresById(int supplierId);
        public Task AddSupplires(SuppliersModel supplier,int createId);
        public Task EditSupplires(SuppliersModel supplier,int updatId);
        public Task DeleteSupplires(int supplierId,int deleteId);
    }
}
