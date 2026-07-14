using ERPDto.StockTransactionDto;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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

            var request = new HttpRequestMessage(HttpMethod.Post, "api/StockTransactions/AddStockTransaction")
            {
                Content = content
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);
            return response;
        }

        public async Task<List<ERPDto.StockTransactionDto.StockTransactionDto>> GetStockTransactionsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/StockTransactions/GetStockTransactions");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ERPDto.StockTransactionDto.StockTransactionDto>>() ?? new List<ERPDto.StockTransactionDto.StockTransactionDto>();
            }
            return new List<ERPDto.StockTransactionDto.StockTransactionDto>();
        }
    }
}
