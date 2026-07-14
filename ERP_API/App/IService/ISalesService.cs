using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.SalesDto;

namespace ERP_API.App.IService
{
    public interface ISalesService
    {
        Task<SaleDto> CompleteSaleAsync(CompleteSaleModel model, int userId);
        Task<SalesListResponse> GetSalesAsync(PageDto page);
        Task<SaleDto> GetSaleByIdAsync(int id);
        Task<ProductDto?> LookupProductByBarcodeAsync(string barcode);
        Task<List<ProductDto>> SearchProductsAsync(string term, int take = 12);
    }
}
