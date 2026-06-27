using ERP_API.Domin.ProductEntity;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;

namespace ERP_API.App.IService
{
    public interface IProductService
    {
        public Task<List<ProductDto>> GetAllProductsAsync(PageDto pageDto);
        public Task<ProductDto> GetProductByIdAsync(int id);
        public Task CreateProduct(CreateProductModel product, int userId);
        public Task UpdateProduct(UpdateProductModel product, int userId);
        public Task DeleteProduct(int id, int userId);
    }
}
