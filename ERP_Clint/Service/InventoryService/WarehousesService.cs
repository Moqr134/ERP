using ERPDto.WarehouseDto;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ERP_Clint.Service;

namespace ERP_Clint.Service.InventoryService
{
    public interface IWarehousesService
    {
        Task<List<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto?> GetWarehouseByIdAsync(int id);
        Task<HttpResponseMessage> CreateWarehouseAsync(WarehouseModel model);
        Task<HttpResponseMessage> UpdateWarehouseAsync(WarehouseModel model);
        Task<HttpResponseMessage> DeleteWarehouseAsync(int id);
    }

    public class WarehousesService : IWarehousesService
    {
        private readonly HttpClient _httpClient;

        public WarehousesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WarehouseDto>> GetAllWarehousesAsync()
        {
            var response = await _httpClient.GetAsync("api/Warehouse/GetAllWarehouses");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المخازن", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<List<WarehouseDto>>() ?? new List<WarehouseDto>();
        }

        public async Task<WarehouseDto?> GetWarehouseByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Warehouse/GetWarehouseById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل المخزن", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<WarehouseDto>();
        }

        public async Task<HttpResponseMessage> CreateWarehouseAsync(WarehouseModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("api/Warehouse/AddWarehouse", content);
        }

        public async Task<HttpResponseMessage> UpdateWarehouseAsync(WarehouseModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync("api/Warehouse/EditWarehouse", content);
        }

        public async Task<HttpResponseMessage> DeleteWarehouseAsync(int id)
        {
            return await _httpClient.DeleteAsync($"api/Warehouse/DeleteWarehouse/{id}");
        }
    }
}
