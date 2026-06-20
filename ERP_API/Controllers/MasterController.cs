using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.UsersEntity;
using ERPDto.UserDto;
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

        public DBContext _context;
        private IAppMemoryCache _cache;
        public int _UserId = 0;
        public readonly IUserService _usersService;
        public readonly Jwt jwt;
        public IMapper _mapper;
        public MasterController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper)
        {
            _context = context;
            _cache = cache;
            jwt = new Jwt(_context);
            _usersService = userService;
            _mapper = mapper;
        }
        public UserOut UserManager
        {
            get
            {
                if (_UserId == 0)
                    GetUserId();
                if (_cache.IsExist("User" + _UserId))
                    return _cache.Get<UserOut>("User" + _UserId);
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
    
        private UserOut ResetUserinfo()
        {

            if (_UserId == 0)
                GetUserId();
            UserManager = _usersService.GetUser(_UserId);
            if (UserManager is null)
                throw new KeyNotFoundException(nameof(_UserId));
            return UserManager;
        }
        protected void GetUserId()
        {
            //_UserId = jwt.ValidateToken(HttpContext.Request.Headers["Authorization"].ToString());
            _UserId = jwt.ValidateToken(HttpContext.Request.Cookies["AuthToken"].ToString());
        }
    }
}
