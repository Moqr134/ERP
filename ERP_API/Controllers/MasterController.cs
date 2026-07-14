using AutoMapper;
using ERP_API.App.IService;
using ERPDto.UserDto;
using Infrastructure.Cache;
using Infrastructure.JWT;
using Infrastructure.ORM;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SecondApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {

        public DBContext _context;
        protected IAppMemoryCache _cache;
        public int _UserId = 0;
        public readonly IUserService _usersService;
        public readonly Jwt jwt;
        public IMapper _mapper;
        public MasterController(IUserService userService, IAppMemoryCache cache, DBContext context, IMapper mapper, Jwt jwtService)
        {
            _context = context;
            _cache = cache;
            jwt = jwtService;
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
                if (_UserId == 0)
                    throw new InvalidOperationException("يجب تحديد معرف المستخدم قبل تحديث الذاكرة المؤقتة");
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
            var idClaim = User?.FindFirst("ID")?.Value
                ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out var claimUserId) && claimUserId > 0)
            {
                _UserId = claimUserId;
                return;
            }

            var token = HttpContext.Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("يجب تسجيل الدخول أولاً");

            _UserId = jwt.ValidateToken(token);
        }
    }
}
