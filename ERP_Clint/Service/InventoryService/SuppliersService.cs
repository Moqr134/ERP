using ERPDto.Suppliers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ERP_Clint.Service;

namespace ERP_Clint.Service.InventoryService
{
    public interface ISuppliersService
    {
        public Task<List<SuppliersDto>> GetAllSuppliersAsync();
        public Task<SuppliersDto?> GetSupplierByIdAsync(int id);
        public Task<HttpResponseMessage> CreateSupplierAsync(SuppliersDto supplier);
        public Task<HttpResponseMessage> UpdateSupplierAsync(SuppliersDto supplier);
        public Task<HttpResponseMessage> DeleteSupplierAsync(int id);
    }

    public class SuppliersService : ISuppliersService
    {
        private readonly HttpClient _httpClient;
        public SuppliersService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> CreateSupplierAsync(SuppliersDto supplier)
        {
            var json = JsonSerializer.Serialize(supplier);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("api/suppliers/AddSuppliers", content);
        }

        public async Task<HttpResponseMessage> DeleteSupplierAsync(int id)
        {
            return await _httpClient.DeleteAsync($"api/suppliers/DeleteSuppliers/{id}");
        }

        public async Task<List<SuppliersDto>> GetAllSuppliersAsync()
        {
            var response = await _httpClient.GetAsync("api/suppliers/GetAllSuppliers");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل الموردين", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<List<SuppliersDto>>() ?? new List<SuppliersDto>();
        }

        public async Task<SuppliersDto?> GetSupplierByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/suppliers/GetSupplierById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المورد", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<SuppliersDto>();
        }

        public async Task<HttpResponseMessage> UpdateSupplierAsync(SuppliersDto supplier)
        {
            var json = JsonSerializer.Serialize(supplier);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync("api/suppliers/EditSuppliers", content);
        }
    }
}
