using ERPDto.StockTransferDto;
using ERPDto.WarehouseDto;
using ERP_Clint.Service;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.InventoryService
{
    public interface IStockTransferService
    {
        Task<List<StockTransferDto>> GetTransfersAsync();
        Task<StockTransferDto?> GetTransferByIdAsync(int id);
        Task<StockTransferDto?> CreateTransferAsync(CreateStockTransferModel model);
    }

    public class StockTransferService : IStockTransferService
    {
        private readonly HttpClient _httpClient;

        public StockTransferService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<StockTransferDto>> GetTransfersAsync()
        {
            var response = await _httpClient.GetAsync("api/StockTransfer/GetTransfers");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل التحويلات", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<List<StockTransferDto>>() ?? new();
        }

        public async Task<StockTransferDto?> GetTransferByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/StockTransfer/GetTransferById/{id}");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل التحويل", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<StockTransferDto>();
        }

        public async Task<StockTransferDto?> CreateTransferAsync(CreateStockTransferModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/StockTransfer/CreateTransfer", content);
            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorAsync(response) ?? "تعذر إنشاء التحويل";
                throw new ApiRequestException(message, response.StatusCode);
            }
            return await response.Content.ReadFromJsonAsync<StockTransferDto>();
        }

        private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("Message", out var msg))
                    return msg.GetString();
                if (doc.RootElement.TryGetProperty("message", out var msg2))
                    return msg2.GetString();
            }
            catch { }
            return null;
        }
    }
}
