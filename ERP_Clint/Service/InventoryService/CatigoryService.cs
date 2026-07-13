using ERPDto.CategoriesDto;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.InventoryService
{
    public interface ICatigoryService
    {
        public Task<List<CategoryDto>> GetAllCategoriesAsync();
        public Task<CategoryDto?> GetCategoryByIdAsync(int id);
        public Task<HttpResponseMessage> CreateCategoryAsync(CategoryDto category);
        public Task<HttpResponseMessage> UpdateCategoryAsync(CategoryDto category);
        public Task<HttpResponseMessage> DeleteCategoryAsync(int id);
    }
    public class CatigoryService : ICatigoryService
    {
        private readonly HttpClient _httpClient;
        public CatigoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> CreateCategoryAsync(CategoryDto category)
        {
            var json = JsonSerializer.Serialize(category);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/categories/CreateCategory")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/categories/DeleteCategory/{id}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/categories/GetAllCategories");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();
            }
            return new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/categories/GetCategoryById/{id}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CategoryDto>();
            }
            return null;
        }

        public async Task<HttpResponseMessage> UpdateCategoryAsync(CategoryDto category)
        {
            var json = JsonSerializer.Serialize(category);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, "api/categories/UpdateCategory")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}
