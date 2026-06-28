using AutoMapper;
using ERP_API.App.IService;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
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
    public class ProductController : MasterController
    {
        private readonly IProductService _productService;
        public ProductController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper,IProductService productService) : base(userService, cache, context, mapper)
        {
            _productService = productService;
        }
        [HttpGet("GetAllProductsAsync")]
        [Authorize]
        public async Task<IActionResult> GetAllProductsAsync([FromQuery] PageDto pageDto)
        {
            List<ProductDto> products = await _productService.GetAllProductsAsync(pageDto);
            return Ok(products);
        }
        [HttpGet("GetProductByIdAsync/{id}")]
        [Authorize]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            ProductDto productDto = await _productService.GetProductByIdAsync(id);
            return Ok(productDto);
        }
        [HttpPost("CreateProduct")]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromBody]CreateProductModel model)
        {
            if (_UserId == 0) GetUserId();
            await _productService.CreateProduct(model,_UserId);
            return Ok("تم انشاء المنتج");
        }
        [HttpPost("UpdateProduct")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductModel model)
        {
            if (_UserId == 0) GetUserId();
            await _productService.UpdateProduct(model, _UserId);
            return Ok("تم تعديل المنتج");
        }
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if(_UserId == 0) GetUserId();
            await _productService.DeleteProduct(id, _UserId);
            return Ok("تم حذف المنتج");
        }
    }
}
