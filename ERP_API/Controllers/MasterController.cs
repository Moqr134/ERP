using ERP_API.App.IService;
using ERP_API.Domin.UsersEntity;
using Infrastructure.Cache;
using Infrastructure.JWT;
using Infrastructure.Logger;
using Infrastructure.ORM;
using Microsoft.AspNetCore.Mvc;

namespace SecondApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {

        public readonly Loger Loger;
        public DBContext _context;
        private IAppMemoryCache _cache;
        public int _UserId = 0;
        public int _categoryId = 0;
        public readonly IUserService _usersService;
        public readonly Jwt jwt;
        public MasterController(IUserService userService, IAppMemoryCache cache, DBContext context)
        {
            _context = context;
            _cache = cache;
            jwt = new Jwt(_context);
            Loger = new Loger();
            _usersService = userService;
        }
        public Users UserManager
        {
            get
            {
                if (_UserId == 0)
                    GetUserId();
                if (_cache.IsExist("User" + _UserId))
                    return _cache.Get<Users>("User" + _UserId);
                else
                {
                    return ResetUserinfo();
                }
            }
            set
            {
                _cache.Set("User" + _UserId, value);
            }
        }
    
        private Users ResetUserinfo()
        {
            try
            {
                if (_UserId == 0)
                    GetUserId();
                UserManager = _usersService.GetUser(_UserId);
                return UserManager;
            }
            catch (Exception ex)
            {
                new Loger().Write(ex, "MasterController => ResetUserinfo");
                throw;
            }
        }
        protected void GetUserId()
        {
            //_UserId = jwt.ValidateToken(HttpContext.Request.Headers["Authorization"].ToString());
            _UserId = jwt.ValidateToken(HttpContext.Request.Cookies["AuthToken"].ToString());
        }
    }
}
