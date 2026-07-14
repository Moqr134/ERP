using ERPDto.ReportsDto;

namespace ERP_API.App.IService
{
    public interface IReportsService
    {
        Task<DashboardOverviewReport> GetOverviewAsync(ReportFilterDto? filter = null);
        Task<ProductsReport> GetProductsReportAsync(ReportFilterDto? filter = null);
        Task<CategoriesReport> GetCategoriesReportAsync();
        Task<UsersReport> GetUsersReportAsync(ReportFilterDto? filter = null);
        Task<SalesReport> GetSalesReportAsync(ReportFilterDto? filter = null);
        Task<StockReport> GetStockReportAsync(ReportFilterDto? filter = null);
    }
}
