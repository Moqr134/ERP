using Domin.TokenDto;
using ERP_API.App.IService;
using ERP_API.Domin.UsersEntity;
using Infrastructure.Cache;
using Infrastructure.ORM;
using Infrastructure.PassowdHashing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecondApi.Controllers;
using SherdProject.DTO;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : MasterController
    {
        private PasswordHashing passwordHashing;
        public AccountController(IUserService userService, IAppMemoryCache cache, DBContext context) : base(userService, cache, context)
        {
            passwordHashing = new PasswordHashing();
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                    return BadRequest("يرجى ادخال الاسم وكلمه المرور");
                Users user = await _usersService.CheckUser(model.Username);
                if (user == null)
                    return BadRequest("عذرا المستخدم غير موجود");
                bool Check = passwordHashing.VerifyPassword(model.Password, user.HashPassword);
                if (!Check)
                    return BadRequest("عذرا كلمه المرور غير صحيحة");
                TokenResponseDto? token = await jwt.CreateTokenResponse(user);
                if (token == null)
                    return BadRequest("حدث خطا ما ");
                user.IsOnline = true;
                user.LastLogin = DateTime.UtcNow.AddHours(3);
                user.Token = token.AccessToken;
                user.RefreshToken = token.RefreshToken;
                _context.Users.Entry(user);
                await _context.SaveChangesAsync();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                };
                Response.Cookies.Append("AuthToken", user.Token, cookieOptions);
                var refreshCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("RefreshToken", user.RefreshToken, refreshCookieOptions);
                _UserId = user.Id;
                UserManager = user;
                //return Ok("تم تسجيل الدخول بنجاح");
                return Ok(user.Token);
            }
            catch (Exception ex)
            {
                await Loger.WriteAsync(ex, "AccountController => Login");
                return BadRequest("حدث خطا ما");
            }
        }
    }
}
