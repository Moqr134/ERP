using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using ERPDto.SalesDto;
using ERP_Clint.Service;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.SalesService
{
    public interface ISalesService
    {
        Task<SaleDto?> CompleteSaleAsync(CompleteSaleModel model);
        Task<SalesListResponse?> GetSalesAsync(PageDto page);
        Task<SaleDto?> GetSaleByIdAsync(int id);
        Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode);
        Task<List<ProductDto>?> SearchProductsAsync(string term);
    }

    public class SalesService : ISalesService
    {
        private readonly HttpClient _httpClient;

        public SalesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SaleDto?> CompleteSaleAsync(CompleteSaleModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Sales/CompleteSale", content);
            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(response) ?? "تعذر إتمام البيع";
                throw new ApiRequestException(message, response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<SaleDto>();
        }

        public async Task<SalesListResponse?> GetSalesAsync(PageDto page)
        {
            var json = JsonSerializer.Serialize(page);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Sales/GetSales", content);
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المبيعات", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<SalesListResponse>();
        }

        public async Task<SaleDto?> GetSaleByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Sales/GetSaleById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الفاتورة", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<SaleDto>();
        }

        public async Task<ProductLookupDto?> LookupProductByBarcodeAsync(string barcode)
        {
            var encoded = Uri.EscapeDataString(barcode);
            var response = await _httpClient.GetAsync($"api/Sales/LookupProductByBarcode/{encoded}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر البحث بالباركود", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<ProductLookupDto>();
        }

        public async Task<List<ProductDto>?> SearchProductsAsync(string term)
        {
            var encoded = Uri.EscapeDataString(term);
            var response = await _httpClient.GetAsync($"api/Sales/SearchProducts?term={encoded}&take=12");
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
                // ignore parse errors
            }

            return null;
        }
    }
}
