using AutoMapper;
using ERP_API.App.IService;
using ERPDto.ReportsDto;
using Infrastructure.Cache;
using Infrastructure.JWT;
using Infrastructure.ORM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondApi.Controllers;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : MasterController
    {
        private readonly IReportsService _reportsService;

        public ReportsController(
            IReportsService reportsService,
            IUserService userService,
            IAppMemoryCache cache,
            DBContext context,
            IMapper mapper,
            Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _reportsService = reportsService;
        }

        [HttpPost("Overview")]
        [Authorize(Roles = "FullAccess,GetReportsOverview")]
        public async Task<IActionResult> Overview([FromBody] ReportFilterDto? filter)
        {
            var report = await _reportsService.GetOverviewAsync(filter);
            return Ok(report);
        }

        [HttpPost("Products")]
        [Authorize(Roles = "FullAccess,GetProductsReport")]
        public async Task<IActionResult> Products([FromBody] ReportFilterDto? filter)
        {
            var report = await _reportsService.GetProductsReportAsync(filter);
            return Ok(report);
        }

        [HttpGet("Categories")]
        [Authorize(Roles = "FullAccess,GetCategoriesReport")]
        public async Task<IActionResult> Categories()
        {
            var report = await _reportsService.GetCategoriesReportAsync();
            return Ok(report);
        }

        [HttpPost("Users")]
        [Authorize(Roles = "FullAccess,GetUsersReport")]
        public async Task<IActionResult> Users([FromBody] ReportFilterDto? filter)
        {
            var report = await _reportsService.GetUsersReportAsync(filter);
            return Ok(report);
        }

        [HttpPost("Sales")]
        [Authorize(Roles = "FullAccess,GetSalesReport")]
        public async Task<IActionResult> Sales([FromBody] ReportFilterDto? filter)
        {
            var report = await _reportsService.GetSalesReportAsync(filter);
            return Ok(report);
        }

        [HttpPost("Stock")]
        [Authorize(Roles = "FullAccess,GetStockReport")]
        public async Task<IActionResult> Stock([FromBody] ReportFilterDto? filter)
        {
            var report = await _reportsService.GetStockReportAsync(filter);
            return Ok(report);
        }
    }
}
