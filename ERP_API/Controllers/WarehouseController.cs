using AutoMapper;
using ERP_API.App.IService;
using ERPDto.WarehouseDto;
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
    public class WarehouseController : MasterController
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(
            IWarehouseService warehouseService,
            IUserService userService,
            IAppMemoryCache cache,
            DBContext context,
            IMapper mapper,
            Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet("GetAllWarehouses")]
        [Authorize(Roles = "FullAccess,GetAllWarehouses")]
        public async Task<IActionResult> GetAllWarehouses()
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            return Ok(warehouses);
        }

        [HttpGet("GetWarehouseById/{id}")]
        [Authorize(Roles = "FullAccess,GetWarehouseById")]
        public async Task<IActionResult> GetWarehouseById(int id)
        {
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            return Ok(warehouse);
        }

        [HttpPost("AddWarehouse")]
        [Authorize(Roles = "FullAccess,AddWarehouse")]
        public async Task<IActionResult> AddWarehouse([FromBody] WarehouseModel model)
        {
            if (_UserId == 0) GetUserId();
            await _warehouseService.AddWarehouseAsync(model, _UserId);
            return Ok();
        }

        [HttpPut("EditWarehouse")]
        [Authorize(Roles = "FullAccess,EditWarehouse")]
        public async Task<IActionResult> EditWarehouse([FromBody] WarehouseModel model)
        {
            if (_UserId == 0) GetUserId();
            await _warehouseService.EditWarehouseAsync(model, _UserId);
            return Ok();
        }

        [HttpDelete("DeleteWarehouse/{id}")]
        [Authorize(Roles = "FullAccess,DeleteWarehouse")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            if (_UserId == 0) GetUserId();
            await _warehouseService.DeleteWarehouseAsync(id, _UserId);
            return Ok();
        }

        [HttpGet("GetStockByWarehouse/{id:int}")]
        [Authorize(Roles = "FullAccess,GetAllWarehouses,GetStockTransactions")]
        public async Task<IActionResult> GetStockByWarehouse(int id)
        {
            var stock = await _warehouseService.GetStockByWarehouseAsync(id);
            return Ok(stock);
        }

        [HttpGet("GetBalancesByProduct/{productId:int}")]
        [Authorize(Roles = "FullAccess,GetAllWarehouses,GetProductByIdAsync")]
        public async Task<IActionResult> GetBalancesByProduct(int productId)
        {
            var balances = await _warehouseService.GetBalancesByProductAsync(productId);
            return Ok(balances);
        }
    }
}
