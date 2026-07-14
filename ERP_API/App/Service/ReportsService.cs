using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Infrastructure.Services;
using ERPDto.ReportsDto;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class ReportsService : MasterService, IReportsService, IScopped
    {
        public ReportsService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<DashboardOverviewReport> GetOverviewAsync(ReportFilterDto? filter = null)
        {
            filter ??= new ReportFilterDto();
            var (from, to) = NormalizeRange(filter);
            var top = NormalizeTop(filter.Top);
            var todayStart = StartOfDay(DateTime.UtcNow.AddHours(3));
            var last7 = todayStart.AddDays(-6);

            var products = _context.Products.AsNoTracking().Where(p => !p.IsRemoved);
            var salesQuery = ApplySalesDateFilter(_context.Sales.AsNoTracking(), from, to);
            var stockQuery = ApplyStockDateFilter(_context.StockTransactions.AsNoTracking(), from, to);

            var report = new DashboardOverviewReport
            {
                TotalProducts = await products.CountAsync(),
                LowStockProducts = await products.CountAsync(p => p.CurrentStock > 0 && p.CurrentStock <= p.MinStockLevel),
                OutOfStockProducts = await products.CountAsync(p => p.CurrentStock == 0),
                InventoryCostValue = await products.Where(p => p.CurrentStock > 0)
                    .SumAsync(p => (double?)(p.CostPrice * p.CurrentStock)) ?? 0,
                InventorySellValue = await products.Where(p => p.CurrentStock > 0)
                    .SumAsync(p => (double?)(p.SellingPrice * p.CurrentStock)) ?? 0,
                TotalCategories = await _context.Categories.AsNoTracking().CountAsync(c => !c.IsRemoved),
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsRemoved),
                ActiveUsers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsRemoved && u.IsActive),
                OnlineUsers = await _context.Users.AsNoTracking().CountAsync(u => !u.IsRemoved && u.IsOnline),
                TotalSuppliers = await _context.Suppliers.AsNoTracking().CountAsync(s => !s.IsRemoved),
                TotalSales = await salesQuery.CountAsync(),
                SalesRevenue = await salesQuery.SumAsync(s => (double?)s.Total) ?? 0,
                SalesDiscount = await salesQuery.SumAsync(s => (double?)s.Discount) ?? 0,
                SalesToday = await _context.Sales.AsNoTracking().CountAsync(s => s.CreateDate >= todayStart),
                RevenueToday = await _context.Sales.AsNoTracking()
                    .Where(s => s.CreateDate >= todayStart)
                    .SumAsync(s => (double?)s.Total) ?? 0,
                StockInCount = await stockQuery.CountAsync(s => s.TransactionType == "In"),
                StockOutCount = await stockQuery.CountAsync(s => s.TransactionType == "Out"),
            };

            var salesLast7 = await _context.Sales.AsNoTracking()
                .Where(s => s.CreateDate >= last7)
                .GroupBy(s => s.CreateDate.Date)
                .Select(g => new DailyMetricDto
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            report.SalesLast7Days = FillDailyGaps(salesLast7, last7, todayStart);

            report.TopSellingProducts = await _context.SaleLines.AsNoTracking()
                .Where(l => l.Sale != null && (!from.HasValue || l.Sale.CreateDate >= from) && (!to.HasValue || l.Sale.CreateDate <= to))
                .GroupBy(l => l.ProductName)
                .Select(g => new NamedMetricDto
                {
                    Name = g.Key,
                    Count = g.Sum(x => x.Quantity),
                    Value = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync();

            report.TopCategoriesByProducts = await _context.Categories.AsNoTracking()
                .Where(c => !c.IsRemoved)
                .Select(c => new NamedMetricDto
                {
                    Name = c.Name,
                    Count = c.Products.Count(p => !p.IsRemoved),
                    Value = c.Products.Where(p => !p.IsRemoved && p.CurrentStock > 0)
                        .Sum(p => p.CostPrice * p.CurrentStock)
                })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync();

            return report;
        }

        public async Task<ProductsReport> GetProductsReportAsync(ReportFilterDto? filter = null)
        {
            filter ??= new ReportFilterDto();
            var top = NormalizeTop(filter.Top);

            var products = await _context.Products.AsNoTracking()
                .Where(p => !p.IsRemoved)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Barcode,
                    p.SKU,
                    p.CurrentStock,
                    p.MinStockLevel,
                    p.CostPrice,
                    p.SellingPrice,
                    CategoryName = p.Categories != null ? p.Categories.Name : null
                })
                .ToListAsync();

            var report = new ProductsReport
            {
                TotalProducts = products.Count,
                LowStockProducts = products.Count(p => p.CurrentStock > 0 && p.CurrentStock <= p.MinStockLevel),
                OutOfStockProducts = products.Count(p => p.CurrentStock == 0),
                InStockProducts = products.Count(p => p.CurrentStock > p.MinStockLevel),
                TotalCostValue = products.Where(p => p.CurrentStock > 0).Sum(p => p.CostPrice * p.CurrentStock),
                TotalSellValue = products.Where(p => p.CurrentStock > 0).Sum(p => p.SellingPrice * p.CurrentStock),
                AverageSellingPrice = products.Count == 0 ? 0 : products.Average(p => p.SellingPrice),
                AverageCostPrice = products.Count == 0 ? 0 : products.Average(p => p.CostPrice),
            };
            report.PotentialProfit = report.TotalSellValue - report.TotalCostValue;

            report.ByCategory = products
                .GroupBy(p => string.IsNullOrWhiteSpace(p.CategoryName) ? "بدون صنف" : p.CategoryName!)
                .Select(g => new NamedMetricDto
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Value = g.Where(p => p.CurrentStock > 0).Sum(p => p.CostPrice * p.CurrentStock)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            report.LowStockItems = products
                .Where(p => p.CurrentStock <= p.MinStockLevel)
                .OrderBy(p => p.CurrentStock)
                .Take(top)
                .Select(p => new ProductReportRow
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Sku = p.SKU,
                    CategoryName = p.CategoryName,
                    CurrentStock = p.CurrentStock,
                    MinStockLevel = p.MinStockLevel,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    StockCostValue = p.CostPrice * p.CurrentStock,
                    StockSellValue = p.SellingPrice * p.CurrentStock
                })
                .ToList();

            report.HighestValueItems = products
                .OrderByDescending(p => p.CostPrice * p.CurrentStock)
                .Take(top)
                .Select(p => new ProductReportRow
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Sku = p.SKU,
                    CategoryName = p.CategoryName,
                    CurrentStock = p.CurrentStock,
                    MinStockLevel = p.MinStockLevel,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    StockCostValue = p.CostPrice * p.CurrentStock,
                    StockSellValue = p.SellingPrice * p.CurrentStock
                })
                .ToList();

            report.TopByStock = products
                .OrderByDescending(p => p.CurrentStock)
                .Take(top)
                .Select(p => new ProductReportRow
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Sku = p.SKU,
                    CategoryName = p.CategoryName,
                    CurrentStock = p.CurrentStock,
                    MinStockLevel = p.MinStockLevel,
                    CostPrice = p.CostPrice,
                    SellingPrice = p.SellingPrice,
                    StockCostValue = p.CostPrice * p.CurrentStock,
                    StockSellValue = p.SellingPrice * p.CurrentStock
                })
                .ToList();

            return report;
        }

        public async Task<CategoriesReport> GetCategoriesReportAsync()
        {
            var items = await _context.Categories.AsNoTracking()
                .Where(c => !c.IsRemoved)
                .Select(c => new CategoryReportRow
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count(p => !p.IsRemoved),
                    TotalStock = c.Products.Where(p => !p.IsRemoved).Sum(p => p.CurrentStock),
                    LowStockCount = c.Products.Count(p => !p.IsRemoved && p.CurrentStock > 0 && p.CurrentStock <= p.MinStockLevel),
                    OutOfStockCount = c.Products.Count(p => !p.IsRemoved && p.CurrentStock == 0),
                    InventoryCostValue = c.Products.Where(p => !p.IsRemoved && p.CurrentStock > 0)
                        .Sum(p => p.CostPrice * p.CurrentStock),
                    InventorySellValue = c.Products.Where(p => !p.IsRemoved && p.CurrentStock > 0)
                        .Sum(p => p.SellingPrice * p.CurrentStock)
                })
                .OrderByDescending(c => c.ProductCount)
                .ToListAsync();

            return new CategoriesReport
            {
                TotalCategories = items.Count,
                CategoriesWithProducts = items.Count(c => c.ProductCount > 0),
                EmptyCategories = items.Count(c => c.ProductCount == 0),
                TotalProducts = items.Sum(c => c.ProductCount),
                TotalInventoryCost = items.Sum(c => c.InventoryCostValue),
                Items = items
            };
        }

        public async Task<UsersReport> GetUsersReportAsync(ReportFilterDto? filter = null)
        {
            filter ??= new ReportFilterDto();
            var (from, to) = NormalizeRange(filter);
            var top = NormalizeTop(filter.Top);

            var users = await _context.Users.AsNoTracking()
                .Where(u => !u.IsRemoved)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.IsActive,
                    u.IsOnline,
                    u.LastLogin,
                    RoleNames = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            var salesByUser = await ApplySalesDateFilter(_context.Sales.AsNoTracking(), from, to)
                .Where(s => s.CreateUserId != null)
                .GroupBy(s => s.CreateUserId!.Value)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var salesMap = salesByUser.ToDictionary(x => x.UserId);

            var rows = users.Select(u =>
            {
                salesMap.TryGetValue(u.Id, out var sales);
                return new UserReportRow
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    IsOnline = u.IsOnline,
                    LastLogin = u.LastLogin,
                    SalesCount = sales?.Count ?? 0,
                    SalesTotal = sales?.Total ?? 0,
                    RoleNames = u.RoleNames
                };
            }).ToList();

            var byRole = users
                .SelectMany(u => u.RoleNames.DefaultIfEmpty("بدون دور").Select(r => r))
                .GroupBy(r => r)
                .Select(g => new NamedMetricDto { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new UsersReport
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                InactiveUsers = users.Count(u => !u.IsActive),
                OnlineUsers = users.Count(u => u.IsOnline),
                UsersWithSales = rows.Count(u => u.SalesCount > 0),
                TopCashiers = rows.OrderByDescending(u => u.SalesTotal).Take(top).ToList(),
                RecentLogins = rows.Where(u => u.LastLogin.HasValue)
                    .OrderByDescending(u => u.LastLogin)
                    .Take(top)
                    .ToList(),
                ByRole = byRole
            };
        }

        public async Task<SalesReport> GetSalesReportAsync(ReportFilterDto? filter = null)
        {
            filter ??= new ReportFilterDto();
            var (from, to) = NormalizeRange(filter);
            var top = NormalizeTop(filter.Top);

            var salesQuery = ApplySalesDateFilter(_context.Sales.AsNoTracking(), from, to);
            var sales = await salesQuery
                .Include(s => s.Lines)
                .OrderByDescending(s => s.CreateDate)
                .ToListAsync();

            var userIds = sales.Where(s => s.CreateUserId.HasValue).Select(s => s.CreateUserId!.Value).Distinct().ToList();
            var userNames = await _context.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var report = new SalesReport
            {
                TotalInvoices = sales.Count,
                GrossSales = sales.Sum(s => s.SubTotal),
                TotalDiscount = sales.Sum(s => s.Discount),
                NetSales = sales.Sum(s => s.Total),
                AverageInvoice = sales.Count == 0 ? 0 : sales.Average(s => s.Total),
                CashInvoices = sales.Count(s => s.PaymentMethod == "Cash"),
                CardInvoices = sales.Count(s => s.PaymentMethod == "Card"),
                CashAmount = sales.Where(s => s.PaymentMethod == "Cash").Sum(s => s.Total),
                CardAmount = sales.Where(s => s.PaymentMethod == "Card").Sum(s => s.Total),
                ItemsSold = sales.SelectMany(s => s.Lines).Sum(l => l.Quantity),
            };

            var daily = sales
                .GroupBy(s => s.CreateDate.Date)
                .Select(g => new DailyMetricDto
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToList();

            if (from.HasValue && to.HasValue)
                report.DailyTrend = FillDailyGaps(daily, from.Value.Date, to.Value.Date);
            else if (daily.Count > 0)
                report.DailyTrend = FillDailyGaps(daily, daily.Min(d => d.Date), daily.Max(d => d.Date));
            else
                report.DailyTrend = daily;

            report.TopProducts = sales.SelectMany(s => s.Lines)
                .GroupBy(l => l.ProductName)
                .Select(g => new NamedMetricDto
                {
                    Name = g.Key,
                    Count = g.Sum(x => x.Quantity),
                    Value = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Value)
                .Take(top)
                .ToList();

            report.TopCashiers = sales
                .Where(s => s.CreateUserId.HasValue)
                .GroupBy(s => s.CreateUserId!.Value)
                .Select(g => new NamedMetricDto
                {
                    Name = userNames.TryGetValue(g.Key, out var name) ? name : $"#{g.Key}",
                    Count = g.Count(),
                    Value = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Value)
                .Take(top)
                .ToList();

            report.RecentSales = sales.Take(top).Select(s => new SaleReportRow
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                PaymentMethod = s.PaymentMethod,
                Total = s.Total,
                Discount = s.Discount,
                CreateDate = s.CreateDate,
                CashierName = s.CreateUserId.HasValue && userNames.TryGetValue(s.CreateUserId.Value, out var n) ? n : null,
                LinesCount = s.Lines?.Count ?? 0
            }).ToList();

            return report;
        }

        public async Task<StockReport> GetStockReportAsync(ReportFilterDto? filter = null)
        {
            filter ??= new ReportFilterDto();
            var (from, to) = NormalizeRange(filter);
            var top = NormalizeTop(filter.Top);

            var query = ApplyStockDateFilter(_context.StockTransactions.AsNoTracking(), from, to);
            var movements = await (
                from s in query
                join p in _context.Products.AsNoTracking() on s.ProductId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                orderby s.CreateDate descending
                select new
                {
                    s.Id,
                    s.ProductId,
                    ProductName = p != null ? p.Name : ("#" + s.ProductId),
                    s.TransactionType,
                    s.Quantity,
                    s.ReferenceId,
                    s.Notes,
                    s.CreateDate
                })
                .ToListAsync();

            var report = new StockReport
            {
                TotalMovements = movements.Count,
                InCount = movements.Count(m => m.TransactionType == "In"),
                OutCount = movements.Count(m => m.TransactionType == "Out"),
                InQuantity = movements.Where(m => m.TransactionType == "In").Sum(m => m.Quantity),
                OutQuantity = movements.Where(m => m.TransactionType == "Out").Sum(m => m.Quantity),
                ByType = movements
                    .GroupBy(m => string.IsNullOrWhiteSpace(m.TransactionType) ? "أخرى" : m.TransactionType)
                    .Select(g => new NamedMetricDto
                    {
                        Name = g.Key == "In" ? "دخول" : g.Key == "Out" ? "خروج" : g.Key,
                        Count = g.Count(),
                        Value = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                TopMovedProducts = movements
                    .GroupBy(m => m.ProductName)
                    .Select(g => new NamedMetricDto
                    {
                        Name = g.Key,
                        Count = g.Count(),
                        Value = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.Value)
                    .Take(top)
                    .ToList(),
                RecentMovements = movements.Take(top).Select(m => new StockMovementReportRow
                {
                    Id = m.Id,
                    ProductName = m.ProductName,
                    TransactionType = m.TransactionType,
                    Quantity = m.Quantity,
                    ReferenceId = m.ReferenceId,
                    Notes = m.Notes,
                    CreateDate = m.CreateDate
                }).ToList()
            };

            var daily = movements
                .GroupBy(m => m.CreateDate.Date)
                .Select(g => new DailyMetricDto
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToList();

            if (from.HasValue && to.HasValue)
                report.DailyTrend = FillDailyGaps(daily, from.Value.Date, to.Value.Date);
            else if (daily.Count > 0)
                report.DailyTrend = FillDailyGaps(daily, daily.Min(d => d.Date), daily.Max(d => d.Date));
            else
                report.DailyTrend = daily;

            return report;
        }

        private static (DateTime? from, DateTime? to) NormalizeRange(ReportFilterDto filter)
        {
            DateTime? from = filter.FromDate?.Date;
            DateTime? to = filter.ToDate?.Date.AddDays(1).AddTicks(-1);
            if (from.HasValue && to.HasValue && from > to)
                (from, to) = (to.Value.Date, from.Value.Date.AddDays(1).AddTicks(-1));
            return (from, to);
        }

        private static int NormalizeTop(int top) => top is < 1 ? 10 : top > 50 ? 50 : top;

        private static DateTime StartOfDay(DateTime dt) => dt.Date;

        private static IQueryable<Domin.SalesEntity.Sale> ApplySalesDateFilter(
            IQueryable<Domin.SalesEntity.Sale> query, DateTime? from, DateTime? to)
        {
            if (from.HasValue) query = query.Where(s => s.CreateDate >= from);
            if (to.HasValue) query = query.Where(s => s.CreateDate <= to);
            return query;
        }

        private static IQueryable<Domin.StockTransactionsEntity.StockTransactions> ApplyStockDateFilter(
            IQueryable<Domin.StockTransactionsEntity.StockTransactions> query, DateTime? from, DateTime? to)
        {
            if (from.HasValue) query = query.Where(s => s.CreateDate >= from);
            if (to.HasValue) query = query.Where(s => s.CreateDate <= to);
            return query;
        }

        private static List<DailyMetricDto> FillDailyGaps(List<DailyMetricDto> source, DateTime from, DateTime to)
        {
            var map = source.ToDictionary(x => x.Date.Date, x => x);
            var result = new List<DailyMetricDto>();
            for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                if (map.TryGetValue(d, out var item))
                    result.Add(item);
                else
                    result.Add(new DailyMetricDto { Date = d, Count = 0, Total = 0 });
            }
            return result;
        }
    }
}
