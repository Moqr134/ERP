using Domin.TokenDto;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.UsersEntity;
using Infrastructure.AppException;
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
            Users user = await _usersService.CheckUser(model.Username);
            bool Check = passwordHashing.VerifyPassword(model.Password, user.HashPassword);
            if (!Check)
                throw new LogicException("عذراً، كلمة المرور غير صحيحة.");
            TokenResponseDto? token = await jwt.CreateTokenResponse(user, user.Permations.ToList());
            if (token == null)
                throw new Exception("حدث خطأ.");
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
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserModel model)
        {
            Users? check = await _usersService.CheckUserExsist(model.Username);
            if (check != null)
                throw new DuplicateException("المستخدم موجود بلفعل");
            string PasswordHasher = passwordHashing.HashPassword(model.Password);
            Users users = new Users
            {
                Username = model.Username,
                HashPassword = PasswordHasher,
                Email = model.Email,
                IsActive = true,
                IsRemoved = false,
                IsOnline = false,
                Role = "User",
            };
            await _context.Users.AddAsync(users);
            await _context.SaveChangesAsync();
            return Ok("تم انشاء الحساب بنجاح يرجى مراجعه تسجيل الدخول");
        }
    }
}
