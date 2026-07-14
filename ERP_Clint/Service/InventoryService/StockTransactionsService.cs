using ERPDto.StockTransactionDto;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ERP_Clint.Service;

namespace ERP_Clint.Service.InventoryService
{
    public interface IStockTransactionsService
    {
        public Task<HttpResponseMessage> AddStockTransaction(CreateStockTransactionsModel model);
        public Task<List<StockTransactionDto>> GetStockTransactionsAsync();
    }

    public class StockTransactionsService : IStockTransactionsService
    {
        private readonly HttpClient _httpClient;
        public StockTransactionsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> AddStockTransaction(CreateStockTransactionsModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("api/StockTransactions/AddStockTransaction", content);
        }

        public async Task<List<StockTransactionDto>> GetStockTransactionsAsync()
        {
            var response = await _httpClient.GetAsync("api/StockTransactions/GetStockTransactions");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل حركات المخزون", response.StatusCode);

            return await response.Content.ReadFromJsonAsync<List<StockTransactionDto>>() ?? new List<StockTransactionDto>();
        }
    }
}
