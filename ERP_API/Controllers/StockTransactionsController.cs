using AutoMapper;
using ERP_API.App.IService;
using ERPDto.StockTransactionDto;
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
    public class StockTransactionsController : MasterController
    {
        private readonly IStockTransactionsService _stockTransactionsService;
        public StockTransactionsController(IStockTransactionsService stockTransactionsService,IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper, Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _stockTransactionsService = stockTransactionsService;
        }
        [HttpPost("AddStockTransaction")]
        [Authorize(Roles = "FullAccess,AddStockTransaction")]
        public async Task<IActionResult> AddStockTransaction([FromBody]CreateStockTransactionsModel model)
        {
            if(_UserId == 0) GetUserId();
            await _stockTransactionsService.AddStockTransaction(model, _UserId);
            return Ok("تمت العمليه بنجاح");
        }

        [HttpGet("GetStockTransactions")]
        [Authorize(Roles = "FullAccess,GetStockTransactions")]
        public async Task<IActionResult> GetStockTransactions()
        {
            var list = await _stockTransactionsService.GetStockTransactionsAsync();
            return Ok(list);
        }
    }
}
