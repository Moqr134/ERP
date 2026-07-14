using ERPDto.ReportsDto;
using ERP_Clint.Service;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ERP_Clint.Service.ReportsService
{
    public interface IReportsService
    {
        Task<DashboardOverviewReport?> GetOverviewAsync(ReportFilterDto? filter = null);
        Task<ProductsReport?> GetProductsReportAsync(ReportFilterDto? filter = null);
        Task<CategoriesReport?> GetCategoriesReportAsync();
        Task<UsersReport?> GetUsersReportAsync(ReportFilterDto? filter = null);
        Task<SalesReport?> GetSalesReportAsync(ReportFilterDto? filter = null);
        Task<StockReport?> GetStockReportAsync(ReportFilterDto? filter = null);
    }

    public class ReportsService : IReportsService
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReportsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<DashboardOverviewReport?> GetOverviewAsync(ReportFilterDto? filter = null)
            => PostAsync<DashboardOverviewReport>("api/Reports/Overview", filter ?? new ReportFilterDto());

        public Task<ProductsReport?> GetProductsReportAsync(ReportFilterDto? filter = null)
            => PostAsync<ProductsReport>("api/Reports/Products", filter ?? new ReportFilterDto());

        public async Task<CategoriesReport?> GetCategoriesReportAsync()
        {
            var response = await _httpClient.GetAsync("api/Reports/Categories");
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل تقرير الأصناف", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<CategoriesReport>(JsonOptions);
        }

        public Task<UsersReport?> GetUsersReportAsync(ReportFilterDto? filter = null)
            => PostAsync<UsersReport>("api/Reports/Users", filter ?? new ReportFilterDto());

        public Task<SalesReport?> GetSalesReportAsync(ReportFilterDto? filter = null)
            => PostAsync<SalesReport>("api/Reports/Sales", filter ?? new ReportFilterDto());

        public Task<StockReport?> GetStockReportAsync(ReportFilterDto? filter = null)
            => PostAsync<StockReport>("api/Reports/Stock", filter ?? new ReportFilterDto());

        private async Task<T?> PostAsync<T>(string url, ReportFilterDto filter)
        {
            var json = JsonSerializer.Serialize(filter);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
                throw new ApiRequestException("تعذر تحميل التقرير", response.StatusCode);
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
    }
}
