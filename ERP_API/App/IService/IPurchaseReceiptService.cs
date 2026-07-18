using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.PurchaseDto;

namespace ERP_API.App.IService
{
    public interface IPurchaseReceiptService
    {
        Task<PurchaseReceiptDto> CompleteReceiptAsync(CompletePurchaseReceiptModel model, int userId);
        Task<PurchaseReceiptListResponse> GetReceiptsAsync(PageDto page);
        Task<PurchaseReceiptDto> GetReceiptByIdAsync(int id);
        Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode, int? warehouseId = null);
        Task<List<ProductDto>> SearchProductsAsync(string term, int take = 12, int? warehouseId = null);
    }
}
