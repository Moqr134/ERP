using AutoMapper;
using Domin.TokenDto;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.UsersEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.UserDto;
using Infrastructure.AppException;
using Infrastructure.JWT;
using Infrastructure.ORM;
using Infrastructure.PassowdHashing;
using Infrastructure.Service;
using SherdProject.DTO;
using System.Security;

namespace ERP_API.App.Service
{
    public class AccountService : MasterService, IAccountService, IScopped
    {
        private readonly IUserService _UserService;
        private PasswordHashing passwordHashing;
        private readonly Jwt jwt;
        public AccountService(DBContext context, IMapper mapper,IUserService userService) : base(context, mapper)
        {
            _UserService = userService;
            passwordHashing = new PasswordHashing();
            jwt = new Jwt(_context);
        }

        public async Task<UserTokenDto> Login(LoginModel Model)
        {
            Users? user = await _UserService.GetFullUser(Model.Username);
            if(user == null)
            {
                throw new KeyNotFoundException("المستخدم غير موجود");
            }
            var checkPassword = passwordHashing.VerifyPassword(Model.Password, user.HashPassword);
            if(!checkPassword)
            {
                throw new UnauthorizedAccessException("كلمة المرور غير صحيحة");
            }
            var permisson = await _UserService.GetUserPermissions(user.Id, user.UserRoles.RoleId);
            TokenResponseDto? token = await jwt.CreateTokenResponse(user, permisson);
            if (token == null)
            {
                throw new Exception("فشل في إنشاء التوكن");
            }
            user.Token = token.AccessToken;
            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            user.IsOnline = true;
            user.LastLogin = DateTime.UtcNow.AddHours(3);
            UserTokenDto userToken = new UserTokenDto();
            _mapper.Map<Users, UserTokenDto>(user, userToken);
            _context.Users.Entry(user);
            await _context.SaveChangesAsync();
            return userToken;
        }
        public async Task Register(RegisterModel Model, int userId)
        {
            Users? ceckUser = await _UserService.CheckUserExsist(Model.Username);
            if (ceckUser != null)
            {
                throw new DuplicateException("المستخدم موجود مسبقا");
            }
            string hashPassword = passwordHashing.HashPassword(Model.Password);
            var role = await _UserService.GetRole(Model.Role);
            var userRoles = new UserRoles
            {
                RoleId = role.Id
            };
            Users users = new Users
            {
                Username = Model.Username,
                HashPassword = hashPassword,
                Email = Model.Email,
                IsActive = true,
                IsRemoved = false,
                IsOnline = false,
                CreateDate = DateTime.UtcNow.AddHours(3),
                CreateUserId = userId,
                UserRoles = userRoles
            };
            _context.Users.Add(users);
            await _context.SaveChangesAsync();
        }
        public async Task<UserTokenDto> RefreshToken(string refreshToken)
        {
            Users? users = await _UserService.GetUserByRefreshToken(refreshToken);
            if (users == null)
            {
                throw new KeyNotFoundException("المستخدم غير موجود");
            }
            var permissions = await _UserService.GetUserPermissions(users.Id, users.UserRoles.RoleId);
            TokenResponseDto? token = await jwt.RefreshTokensAsync(users, permissions);
            if (token == null)
            {
                throw new Exception("فشل في إنشاء التوكن");
            }
            users.Token = token.AccessToken;
            users.RefreshToken = token.RefreshToken;
            users.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            users.IsOnline = true;
            users.LastLogin = DateTime.UtcNow.AddHours(3);
            UserTokenDto userToken = new UserTokenDto();
            _mapper.Map<Users, UserTokenDto>(users, userToken);
            _context.Users.Entry(users);
            await _context.SaveChangesAsync();
            return userToken;
        }
    }
}
