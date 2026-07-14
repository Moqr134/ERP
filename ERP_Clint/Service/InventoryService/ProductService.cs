using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ERP_Clint.Service;

namespace ERP_Clint.Service.InventoryService
{
    public interface IProductService
    {
        public Task<List<ProductDto>?> GetAllProductsAsync(PageDto page);
        public Task<ProductDto?> GetProductByIdAsync(int id);
        public Task<ProductsInfo?> GetProductsInfo(PageDto page);
        public Task<HttpResponseMessage> CreateProduct(CreateProductModel model);
        public Task<HttpResponseMessage> UpdateProduct(UpdateProductModel model);
        public Task<HttpResponseMessage> DeleteProduct(int id);
        public Task<ProductDto?> GetProductByBarcodeAsync(string barcode);
    }
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> CreateProduct(CreateProductModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("api/Product/CreateProduct", content);
        }

        public async Task<HttpResponseMessage> DeleteProduct(int id)
        {
            return await _httpClient.DeleteAsync($"api/Product/DeleteProduct/{id}");
        }

        public async Task<List<ProductDto>?> GetAllProductsAsync(PageDto page)
        {
            var json = JsonSerializer.Serialize(page);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Product/GetAllProductsAsync", content);
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المنتجات", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Product/GetProductByIdAsync/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المنتج", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task<ProductsInfo?> GetProductsInfo(PageDto page)
        {
            var json = JsonSerializer.Serialize(page);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Product/GetProductsInfo", content);
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل معلومات المنتجات", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<ProductsInfo>();
        }

        public async Task<HttpResponseMessage> UpdateProduct(UpdateProductModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync("api/Product/UpdateProduct", content);
        }

        public async Task<ProductDto?> GetProductByBarcodeAsync(string barcode)
        {
            var encoded = Uri.EscapeDataString(barcode);
            var response = await _httpClient.GetAsync($"api/Product/GetProductByBarcode/{encoded}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر البحث بالباركود", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }
    }
}
