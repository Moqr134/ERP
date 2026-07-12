using ERPDto.CategoriesDto;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.InventoryService
{
    public interface IProductService
    {
        public Task<List<ProductDto>?> GetAllProductsAsync(PageDto page);
        public Task<ProductDto?> GetProductByIdAsync(int id);
        public Task<ProductsInfo?> GetProductsInfo();
        public Task<HttpResponseMessage> CreateProduct(CreateProductModel model);
        public Task<HttpResponseMessage> UpdateProduct(UpdateProductModel model);
        public Task<HttpResponseMessage> DeleteProduct(int id);
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

            var request = new HttpRequestMessage(HttpMethod.Post, "api/Product/CreateProduct")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteProduct(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Product/DeleteProduct/{id}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<List<ProductDto>?> GetAllProductsAsync(PageDto page)
        {
            var json = JsonSerializer.Serialize(page);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/Product/GetAllProductsAsync")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            if(response.IsSuccessStatusCode)
            {
                List<ProductDto>? Products = response.Content.ReadFromJsonAsync<List<ProductDto>>().Result;
                return Products;
            }
            return new List<ProductDto>();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Product/GetProductByIdAsync/{id}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                ProductDto? Product = response.Content.ReadFromJsonAsync<ProductDto>().Result;
                return Product;
            }
            return new ProductDto();
        }

        public async Task<ProductsInfo?> GetProductsInfo()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Product/GetProductsInfo");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                ProductsInfo? ProductsInfo = response.Content.ReadFromJsonAsync<ProductsInfo>().Result;
                return ProductsInfo;
            }
            return await Task.FromResult(new ProductsInfo());
        }

        public async Task<HttpResponseMessage> UpdateProduct(UpdateProductModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, "api/Product/UpdateProduct")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}
