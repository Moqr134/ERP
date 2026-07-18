using ERP_API.Domin.ProductEntity;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;

namespace ERP_API.App.IService
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync(PageDto pageDto);
        Task<Product?> GetProductByBarcode(string Barcode);
        Task<ProductLookupDto?> LookupByBarcodeAsync(string barcode, int? warehouseId = null);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task CreateProduct(CreateProductModel product, int userId);
        Task UpdateProduct(UpdateProductModel product, int userId);
        Task<List<ProductStockLadgerDto>> GetProductStockLedger(int id);
        Task<List<ProductDto>> GetLowStockProduct();
        Task<ProductsInfo> GetProductsInfo(PageDto pageDto);
        Task DeleteProduct(int id, int userId);
        Task<List<ProductDto>> SearchProductsAsync(string term, int take = 12, int? warehouseId = null);

        /// <summary>Invalidate cached product reads (lists, lookups, details, POS search).</summary>
        void InvalidateProductCache();
    }
}
