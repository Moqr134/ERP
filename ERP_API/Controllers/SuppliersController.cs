using AutoMapper;
using ERP_API.App.IService;
using ERPDto.Suppliers;
using Infrastructure.Cache;
using Infrastructure.ORM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecondApi.Controllers;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : MasterController
    {
        private readonly ISuppliersService _suppliersService;
        public SuppliersController(IUserService userService,ISuppliersService suppliersServise, IAppMemoryCache cache, DBContext context, IMapper mapper) : base(userService, cache, context, mapper)
        {
            _suppliersService = suppliersServise;
        }
        [HttpGet("GetAllSuppliers")]
        [Authorize]
        public async Task<IActionResult> GetAllSuppliers()
        {
            List<SuppliersDto> suppliers = await _suppliersService.GetAllSupplires();
            return Ok(suppliers);
        }
        [HttpGet("GetSupplierById/{id}")]
        [Authorize]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            SuppliersDto supplier = await _suppliersService.GetSuppliresById(id);
            return Ok(supplier);
        }
        [HttpPost("AddSuppliers")]
        [Authorize]
        public async Task<IActionResult> AddSuppliers([FromBody] SuppliersModel supplier)
        {
            if (_UserId == 0) GetUserId();
            await _suppliersService.AddSupplires(supplier, _UserId);
            return Ok();
        }
        [HttpPut("EditSuppliers")]
        [Authorize]
        public async Task<IActionResult> EditSuppliers([FromBody] SuppliersModel supplier)
        {
            if (_UserId == 0) GetUserId();
            await _suppliersService.EditSupplires(supplier, _UserId);
            return Ok();
        }
        [HttpDelete("DeleteSuppliers/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSuppliers(int id)
        {
            if (_UserId == 0) GetUserId();
            await _suppliersService.DeleteSupplires(id, _UserId);
            return Ok();
        }
    }
}
