using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.PurchaseDto;
using ERP_Clint.Service;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.PurchasesService
{
    public interface IPurchasesService
    {
        Task<PurchaseReceiptDto?> CompleteReceiptAsync(CompletePurchaseReceiptModel model);
        Task<PurchaseReceiptListResponse?> GetReceiptsAsync(PageDto page);
        Task<PurchaseReceiptDto?> GetReceiptByIdAsync(int id);
        Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode, int? warehouseId = null);
        Task<List<ProductDto>?> SearchProductsAsync(string term, int? warehouseId = null);
    }

    public class PurchasesService : IPurchasesService
    {
        private readonly HttpClient _httpClient;

        public PurchasesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PurchaseReceiptDto?> CompleteReceiptAsync(CompletePurchaseReceiptModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Purchases/CompleteReceipt", content);
            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(response) ?? "تعذر إتمام الاستلام";
                throw new ApiRequestException(message, response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<PurchaseReceiptDto>();
        }

        public async Task<PurchaseReceiptListResponse?> GetReceiptsAsync(PageDto page)
        {
            var json = JsonSerializer.Serialize(page);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Purchases/GetReceipts", content);
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل سندات الاستلام", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<PurchaseReceiptListResponse>();
        }

        public async Task<PurchaseReceiptDto?> GetReceiptByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Purchases/GetReceiptById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل السند", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<PurchaseReceiptDto>();
        }

        public async Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode, int? warehouseId = null)
        {
            var encoded = Uri.EscapeDataString(barcode);
            var qs = warehouseId is > 0 ? $"?warehouseId={warehouseId.Value}" : string.Empty;
            var response = await _httpClient.GetAsync($"api/Purchases/LookupProductByBarcode/{encoded}{qs}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر البحث بالباركود", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<ProductLookupDto>();
        }

        public async Task<List<ProductDto>?> SearchProductsAsync(string term, int? warehouseId = null)
        {
            var encoded = Uri.EscapeDataString(term);
            var qs = warehouseId is > 0 ? $"&warehouseId={warehouseId.Value}" : string.Empty;
            var response = await _httpClient.GetAsync($"api/Purchases/SearchProducts?term={encoded}&take=12{qs}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر البحث عن المنتجات", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        }

        private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("Message", out var msg))
                    return msg.GetString();
                if (doc.RootElement.TryGetProperty("message", out var msg2))
                    return msg2.GetString();
            }
            catch
            {
                // ignore
            }

            return null;
        }
    }
}
