using ERPDto.CategoriesDto;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ERP_Clint.Service;

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
            return await _httpClient.PostAsync("api/categories/CreateCategory", content);
        }

        public async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            return await _httpClient.DeleteAsync($"api/categories/DeleteCategory/{id}");
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var response = await _httpClient.GetAsync("api/categories/GetAllCategories");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الأصناف", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new List<CategoryDto>();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/categories/GetCategoryById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الصنف", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<CategoryDto>();
        }

        public async Task<HttpResponseMessage> UpdateCategoryAsync(CategoryDto category)
        {
            var json = JsonSerializer.Serialize(category);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync("api/categories/UpdateCategory", content);
        }
    }
}
