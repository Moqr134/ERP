using AutoMapper;
using ERP_API.App.IService;
using ERPDto.PaigingDto;
using ERPDto.SalesDto;
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
    public class SalesController : MasterController
    {
        private readonly ISalesService _salesService;

        public SalesController(
            ISalesService salesService,
            IUserService userService,
            IAppMemoryCache cache,
            DBContext context,
            IMapper mapper,
            Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _salesService = salesService;
        }

        [HttpPost("CompleteSale")]
        [Authorize(Roles = "FullAccess,CompleteSale")]
        public async Task<IActionResult> CompleteSale([FromBody] CompleteSaleModel model)
        {
            if (_UserId == 0) GetUserId();
            var sale = await _salesService.CompleteSaleAsync(model, _UserId);
            return Ok(sale);
        }

        [HttpGet("LookupProductByBarcode/{barcode}")]
        [Authorize(Roles = "FullAccess,CompleteSale")]
        public async Task<IActionResult> LookupProductByBarcode(string barcode)
        {
            var product = await _salesService.LookupProductByBarcodeAsync(barcode);
            if (product is null)
                throw new KeyNotFoundException("لم يتم العثور على المادة");
            return Ok(product);
        }

        [HttpGet("SearchProducts")]
        [Authorize(Roles = "FullAccess,CompleteSale")]
        public async Task<IActionResult> SearchProducts([FromQuery] string term, [FromQuery] int take = 12)
        {
            var products = await _salesService.SearchProductsAsync(term, take);
            return Ok(products);
        }

        [HttpPost("GetSales")]
        [Authorize(Roles = "FullAccess,GetSales")]
        public async Task<IActionResult> GetSales([FromBody] PageDto page)
        {
            var result = await _salesService.GetSalesAsync(page);
            return Ok(result);
        }

        [HttpGet("GetSaleById/{id:int}")]
        [Authorize(Roles = "FullAccess,GetSaleById")]
        public async Task<IActionResult> GetSaleById(int id)
        {
            var sale = await _salesService.GetSaleByIdAsync(id);
            return Ok(sale);
        }
    }
}
