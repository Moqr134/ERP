using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.RoleEntity;
using ERPDto.RolesDto;
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
    public class RoleController : MasterController
    {
        private readonly IRoleService _roleService;
        public RoleController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper,IRoleService roleService, Jwt jwtService) : base(userService, cache, context, mapper, jwtService)
        {
            _roleService = roleService;
        }
        [HttpGet("GetAllRoles")]
        [Authorize(Roles = "FullAccess,GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            List<RoleDto> roles = await _roleService.GetAllRoles();
            return Ok(roles);
        }
        [HttpGet("GetRoleById/{id}")]
        [Authorize(Roles = "FullAccess,GetRoleById")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            Role? role = await _roleService.GetRoleById(id);
            if (role == null)
            {
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");
            }
            RoleDto roleDto = _mapper.Map<RoleDto>(role);
            return Ok(roleDto);
        }
        [HttpPost("CreateRole")]
        [Authorize(Roles = "FullAccess,CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            if (_UserId == 0) GetUserId();
            await _roleService.CreateRole(roleDto, _UserId);
            return Ok();
        }
        [HttpPut("UpdateRole")]
        [Authorize(Roles = "FullAccess,UpdateRole")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleDto roleDto)
        {
            if (_UserId == 0) GetUserId();
            await _roleService.UpdateRole(_UserId, roleDto);
            return Ok();
        }
        [HttpDelete("DeleteRole/{id}")]
        [Authorize(Roles = "FullAccess,DeleteRole")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            if (_UserId == 0) GetUserId();
            await _roleService.DeleteRole(id, _UserId);
            return Ok();
        }
        [HttpPost("CreateRolePermission")]
        [Authorize(Roles = "FullAccess,CreateRolePermission")]
        public async Task<IActionResult> CreateRolePermission([FromBody] RolePermissionDto rolePermissionDto)
        {
            await _roleService.CreateRolePermission(rolePermissionDto.RoleId, rolePermissionDto.PermissionIds);
            return Ok();
        }
    }
}
