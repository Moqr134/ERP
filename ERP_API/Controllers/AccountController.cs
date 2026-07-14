using AutoMapper;
using Domin.TokenDto;
using ERP_API.App.IService;
using ERPDto.UserDto;
using Infrastructure.Cache;
using Infrastructure.JWT;
using Infrastructure.ORM;
using Microsoft.AspNetCore.Authorization;
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
        public AccountController(IUserService userService, IAppMemoryCache cache, DBContext context, IAccountService accountService, IMapper mapper, Jwt jwtService)
            : base(userService, cache, context, mapper, jwtService)
        {
            _accountService = accountService;
        }

        private CookieOptions CreateAuthCookieOptions() => new()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes)
        };

        private CookieOptions CreateRefreshCookieOptions() => new()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = DateTime.UtcNow.AddDays(jwt.RefreshTokenDays)
        };

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel userModel)
        {
            var token = await _accountService.Login(userModel);
            var user = _mapper.Map<UserOut>(token);
            Response.Cookies.Append("AuthToken", token.Token, CreateAuthCookieOptions());
            Response.Cookies.Append("RefreshToken", token.RefreshToken, CreateRefreshCookieOptions());
            _UserId = user.Id;
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
            Response.Cookies.Append("AuthToken", token.Token, CreateAuthCookieOptions());
            Response.Cookies.Append("RefreshToken", token.RefreshToken, CreateRefreshCookieOptions());
            _UserId = user.Id;
            UserManager = user;
            return Ok("تم تجديد الجلسة بنجاح");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (_UserId == 0) GetUserId();
            await _accountService.Logout(_UserId);
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");
            _cache.Remove("User" + _UserId);
            return Ok("تم تسجيل الخروج بنجاح");
        }

        [HttpGet("userinfo")]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type == "USERNAME")?.Value
                ?? "User";

            var userInfo = new UserInfoResponse
            {
                IsAuthenticated = true,
                UserName = userName,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                Roles = roles
            };

            return Ok(userInfo);
        }
    }
}
