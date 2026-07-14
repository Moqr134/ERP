using AutoMapper;
using ERP_API.App.IService;
using ERPDto.PaigingDto;
using ERPDto.UserDto;
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
    public class UserController : MasterController
    {
        public UserController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper, Jwt jwtService)
            : base(userService, cache, context, mapper, jwtService)
        {
        }

        [HttpPost("GetUsers")]
        [Authorize(Roles = "FullAccess,GetUsers")]
        public async Task<IActionResult> GetUsers([FromBody] PageDto dto)
        {
            var users = await _usersService.GetUsersPaged(dto);
            return Ok(users);
        }

        [HttpGet("GetUserById/{id}")]
        [Authorize(Roles = "FullAccess,GetUserById")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _usersService.GetUserDetail(id);
            return Ok(user);
        }

        [HttpGet("GetUsersInfo")]
        [Authorize(Roles = "FullAccess,GetUsersInfo")]
        public async Task<IActionResult> GetUsersInfo()
        {
            var info = await _usersService.GetUsersInfo();
            return Ok(info);
        }

        [HttpPost("CreateUser")]
        [Authorize(Roles = "FullAccess,CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.CreateUser(model, _UserId);
            return Ok("تم إنشاء المستخدم بنجاح");
        }

        [HttpPut("UpdateUser")]
        [Authorize(Roles = "FullAccess,UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.UpdateUser(model, _UserId);
            return Ok("تم تعديل المستخدم بنجاح");
        }

        [HttpPut("SetUserActive")]
        [Authorize(Roles = "FullAccess,SetUserActive")]
        public async Task<IActionResult> SetUserActive([FromBody] SetUserActiveDto model)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.SetUserActive(model.UserId, model.IsActive, _UserId);
            return Ok(model.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم");
        }

        [HttpPut("AssignUserRoles")]
        [Authorize(Roles = "FullAccess,AssignUserRoles")]
        public async Task<IActionResult> AssignUserRoles([FromBody] AssignUserRolesDto model)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.AssignUserRoles(model.UserId, model.RoleIds, _UserId);
            return Ok("تم تحديث أدوار المستخدم");
        }

        [HttpPut("ChangePassword")]
        [Authorize(Roles = "FullAccess,ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.ChangePassword(model.UserId, model.NewPassword, _UserId);
            return Ok("تم تغيير كلمة المرور بنجاح");
        }

        [HttpGet("GetUserPermissions/{userId}")]
        [Authorize(Roles = "FullAccess,GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var permissions = await _usersService.GetUserPermissionsView(userId);
            return Ok(permissions);
        }

        [HttpPut("UpdateUserPermission/{userId}")]
        [Authorize(Roles = "FullAccess,UpdateUserPermission")]
        public async Task<IActionResult> UpdateUserPermission(int userId, [FromBody] List<UserPermissionDto> userPermission)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.UpdateUserpermission(userId, userPermission ?? new());
            return Ok("تم تعديل صلاحيات المستخدم بنجاح");
        }

        [HttpDelete("DeleteUser/{userId}")]
        [Authorize(Roles = "FullAccess,DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.DeleteUser(userId, _UserId);
            return Ok("تم حذف المستخدم بنجاح");
        }
    }
}
