namespace ERPDto.ReportsDto
{
    public class ReportFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Top { get; set; } = 10;
    }

    public class NamedMetricDto
    {
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public int Count { get; set; }
    }

    public class DailyMetricDto
    {
        public DateTime Date { get; set; }
        public double Total { get; set; }
        public int Count { get; set; }
    }

    public class DashboardOverviewReport
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public double InventoryCostValue { get; set; }
        public double InventorySellValue { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalSales { get; set; }
        public double SalesRevenue { get; set; }
        public double SalesDiscount { get; set; }
        public int SalesToday { get; set; }
        public double RevenueToday { get; set; }
        public int StockInCount { get; set; }
        public int StockOutCount { get; set; }
        public List<DailyMetricDto> SalesLast7Days { get; set; } = new();
        public List<NamedMetricDto> TopSellingProducts { get; set; } = new();
        public List<NamedMetricDto> TopCategoriesByProducts { get; set; } = new();
    }

    public class ProductsReport
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int InStockProducts { get; set; }
        public double TotalCostValue { get; set; }
        public double TotalSellValue { get; set; }
        public double PotentialProfit { get; set; }
        public double AverageSellingPrice { get; set; }
        public double AverageCostPrice { get; set; }
        public List<NamedMetricDto> ByCategory { get; set; } = new();
        public List<ProductReportRow> LowStockItems { get; set; } = new();
        public List<ProductReportRow> HighestValueItems { get; set; } = new();
        public List<ProductReportRow> TopByStock { get; set; } = new();
    }

    public class ProductReportRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Sku { get; set; }
        public string? CategoryName { get; set; }
        public int CurrentStock { get; set; }
        public int MinStockLevel { get; set; }
        public double CostPrice { get; set; }
        public double SellingPrice { get; set; }
        public double StockCostValue { get; set; }
        public double StockSellValue { get; set; }
    }

    public class CategoriesReport
    {
        public int TotalCategories { get; set; }
        public int CategoriesWithProducts { get; set; }
        public int EmptyCategories { get; set; }
        public int TotalProducts { get; set; }
        public double TotalInventoryCost { get; set; }
        public List<CategoryReportRow> Items { get; set; } = new();
    }

    public class CategoryReportRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public double InventoryCostValue { get; set; }
        public double InventorySellValue { get; set; }
    }

    public class UsersReport
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int UsersWithSales { get; set; }
        public List<UserReportRow> TopCashiers { get; set; } = new();
        public List<UserReportRow> RecentLogins { get; set; } = new();
        public List<NamedMetricDto> ByRole { get; set; } = new();
    }

    public class UserReportRow
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastLogin { get; set; }
        public int SalesCount { get; set; }
        public double SalesTotal { get; set; }
        public List<string> RoleNames { get; set; } = new();
    }

    public class SalesReport
    {
        public int TotalInvoices { get; set; }
        public double GrossSales { get; set; }
        public double TotalDiscount { get; set; }
        public double NetSales { get; set; }
        public double AverageInvoice { get; set; }
        public int CashInvoices { get; set; }
        public int CardInvoices { get; set; }
        public double CashAmount { get; set; }
        public double CardAmount { get; set; }
        public int ItemsSold { get; set; }
        public List<DailyMetricDto> DailyTrend { get; set; } = new();
        public List<NamedMetricDto> TopProducts { get; set; } = new();
        public List<NamedMetricDto> TopCashiers { get; set; } = new();
        public List<SaleReportRow> RecentSales { get; set; } = new();
    }

    public class SaleReportRow
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public double Total { get; set; }
        public double Discount { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CashierName { get; set; }
        public int LinesCount { get; set; }
    }

    public class StockReport
    {
        public int TotalMovements { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public int InQuantity { get; set; }
        public int OutQuantity { get; set; }
        public List<NamedMetricDto> ByType { get; set; } = new();
        public List<NamedMetricDto> TopMovedProducts { get; set; } = new();
        public List<DailyMetricDto> DailyTrend { get; set; } = new();
        public List<StockMovementReportRow> RecentMovements { get; set; } = new();
    }

    public class StockMovementReportRow
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? ReferenceId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
