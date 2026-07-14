using ERPDto.Suppliers;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

            var request = new HttpRequestMessage(HttpMethod.Post, "api/suppliers/AddSuppliers")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteSupplierAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/suppliers/DeleteSuppliers/{id}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<List<SuppliersDto>> GetAllSuppliersAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/suppliers/GetAllSuppliers");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SuppliersDto>>() ?? new List<SuppliersDto>();
            }
            return new List<SuppliersDto>();
        }

        public async Task<SuppliersDto?> GetSupplierByIdAsync(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/suppliers/GetSupplierById/{id}");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SuppliersDto>();
            }
            return null;
        }

        public async Task<HttpResponseMessage> UpdateSupplierAsync(SuppliersDto supplier)
        {
            var json = JsonSerializer.Serialize(supplier);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, "api/suppliers/EditSuppliers")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }
    }
}
