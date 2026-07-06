using AutoMapper;
using ERP_API.App.IService;
using ERPDto.PaigingDto;
using ERPDto.UserDto;
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
    [Authorize(Roles = "FullAccess")]
    public class UserController : MasterController
    {
        public UserController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper) : base(userService, cache, context, mapper)
        {
        }
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers([FromQuery]PageDto dto)
        {
            List<UserOut> users = await _usersService.GetUsers(dto);
            return Ok(users);
        }
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody]UpdateUserModel model)
        {
            if(_UserId==0) GetUserId();
            await _usersService.UpdateUser(model, _UserId);
            return Ok("تم تعديل المستخدم بنجاح");
        }
        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute]int userId)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.DeleteUser(userId, _UserId);
            return Ok("تم حذف المستخدم بنجاح");
        }
        [HttpPut("UpdateUserPermission")]
        public async Task<IActionResult> UpdateUserPermission([FromBody]List<UserPermissionDto> userPermission)
        {
            if (_UserId == 0) GetUserId();
            await _usersService.UpdateUserpermission(userPermission);
            return Ok("تم تعديل صلاحيات المستخدم بنجاح");
        }
    }
}
