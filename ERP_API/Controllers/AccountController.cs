using AutoMapper;
using Domin.TokenDto;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.UsersEntity;
using ERPDto.UserDto;
using Infrastructure.AppException;
using Infrastructure.Cache;
using Infrastructure.ORM;
using Infrastructure.PassowdHashing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecondApi.Controllers;
using SherdProject.DTO;
using System.Security.Claims;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : MasterController
    {
        private readonly IAccountService _accountService;
        public AccountController(IUserService userService, IAppMemoryCache cache, DBContext context,IAccountService accountService,IMapper mapper)
            : base(userService, cache, context,mapper)
        {
            _accountService = accountService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel userModel)
        {
            var token = await _accountService.Login(userModel);
            var user = _mapper.Map<UserOut>(token);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };
            Response.Cookies.Append("AuthToken", token.Token, cookieOptions);
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("RefreshToken", token.RefreshToken, refreshCookieOptions);
            UserManager = user;
            return Ok("تم تسجيل الدخول بنجاح");
        }
        [HttpPost("register")]
        [Authorize(Roles = "FullAccess")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (_UserId == 0) GetUserId();
            await _accountService.Register(model, _UserId);
            return Ok("تم انشاء الحساب بنجاح");
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            string? refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();
            UserTokenDto token = await _accountService.RefreshToken(refreshToken);
            var user = _mapper.Map<UserOut>(token);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };
            Response.Cookies.Append("AuthToken", token.Token, cookieOptions);
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("RefreshToken", token.RefreshToken, refreshCookieOptions);
            UserManager = user;
            return Ok("تم تسجيل الدخول بنجاح");
        }
        [HttpGet("userinfo")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            var userInfo = new UserInfoResponse
            {
                IsAuthenticated = true,
                UserName = User.Identity?.Name ?? "User",
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                Roles = roles
            };

            return Ok(userInfo);
        }
    }
}
