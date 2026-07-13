using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.ProductEntity;
using ERPDto.PaigingDto;
using ERPDto.ProductsDto;
using Infrastructure.Cache;
using Infrastructure.JWT;
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
        public ProductController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper, IProductService productService, Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _productService = productService;
        }
        [HttpPost("GetAllProductsAsync")]
        [Authorize]
        public async Task<IActionResult> GetAllProductsAsync([FromBody] PageDto pageDto)
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
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductModel model)
        {
            if (_UserId == 0) GetUserId();
            await _productService.CreateProduct(model, _UserId);
            return Ok("تم انشاء المنتج");
        }
        [HttpPut("UpdateProduct")]
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
            if (_UserId == 0) GetUserId();
            await _productService.DeleteProduct(id, _UserId);
            return Ok("تم حذف المنتج");
        }
        [HttpGet("GetProductByBarcode/{Barcode}")]
        [Authorize]
        public async Task<IActionResult> GetProductByBarcode(string Barcode)
        {
            Product? product = await _productService.GetProductByBarcode(Barcode);
            if(product == null) throw new KeyNotFoundException("لم يتم العثور على المادة");
            ProductDto dto = _mapper.Map<ProductDto>(product);
            return Ok(dto);
        }
        [HttpGet("GetProductStockLedger/{id}")]
        [Authorize]
        public async Task<IActionResult> GetProductById(int id)
        {
            List<ProductStockLadgerDto> dtos = await _productService.GetProductStockLedger(id);
            return Ok(dtos);
        }
        [HttpGet("GetLowStockProduct")]
        [Authorize]
        public async Task<IActionResult> GetLowStockProduct()
        {
            List<ProductDto> dtos = await _productService.GetLowStockProduct();
            return Ok(dtos);
        }
        [HttpGet("GetProductsInfo")]
        [Authorize]
        public async Task<IActionResult> GetProductsInfo()
        {
            ProductsInfo info = await _productService.GetProductsInfo();
            return Ok(info);
        }
        
    }
}
