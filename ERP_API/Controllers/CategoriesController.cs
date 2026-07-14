using AutoMapper;
using ERP_API.App.IService;
using ERPDto.CategoriesDto;
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
    public class CategoriesController : MasterController
    {
        private ICategoriesService _categoriesService;
        public CategoriesController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper, ICategoriesService categoriesService, Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _categoriesService = categoriesService;
        }
        [HttpGet("GetAllCategories")]
        [Authorize(Roles = "FullAccess,GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            List<CategoryDto> categories = await _categoriesService.GetAllCategories();
            return Ok(categories);
        }
        [HttpGet("GetCategoryById/{id}")]
        [Authorize(Roles = "FullAccess,GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            CategoryDto category = await _categoriesService.GetCategoryById(id);
            return Ok(category);
        }
        [HttpPost("CreateCategory")]
        [Authorize(Roles = "FullAccess,CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto category)
        {
            if (_UserId == 0) GetUserId();
            await _categoriesService.CreateCategory(category, _UserId);
            return Ok();
        }
        [HttpPut("UpdateCategory")]
        [Authorize(Roles = "FullAccess,UpdateCategory")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryDto category)
        {
            if (_UserId == 0) GetUserId();
            await _categoriesService.UpdateCategory(category, _UserId);
            return Ok();
        }
        [HttpDelete("DeleteCategory/{id}")]
        [Authorize(Roles = "FullAccess,DeleteCategory")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            if (_UserId == 0) GetUserId();
            await _categoriesService.DeleteCategory(id, _UserId);
            return Ok();
        }
    }
}
