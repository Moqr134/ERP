using AutoMapper;
using ERP_API.App.IService;
using ERPDto.StockTransferDto;
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
    public class StockTransferController : MasterController
    {
        private readonly IStockTransferService _stockTransferService;

        public StockTransferController(
            IStockTransferService stockTransferService,
            IUserService userService,
            IAppMemoryCache cache,
            DBContext context,
            IMapper mapper,
            Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _stockTransferService = stockTransferService;
        }

        [HttpPost("CreateTransfer")]
        [Authorize(Roles = "FullAccess,CreateStockTransfer")]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferModel model)
        {
            if (_UserId == 0) GetUserId();
            var transfer = await _stockTransferService.CreateTransferAsync(model, _UserId);
            return Ok(transfer);
        }

        [HttpGet("GetTransfers")]
        [Authorize(Roles = "FullAccess,GetStockTransfers")]
        public async Task<IActionResult> GetTransfers()
        {
            var list = await _stockTransferService.GetTransfersAsync();
            return Ok(list);
        }

        [HttpGet("GetTransferById/{id:int}")]
        [Authorize(Roles = "FullAccess,GetStockTransfers")]
        public async Task<IActionResult> GetTransferById(int id)
        {
            var transfer = await _stockTransferService.GetTransferByIdAsync(id);
            return Ok(transfer);
        }
    }
}
