using AutoMapper;
using Domin.TokenDto;
using ERP_API.App.IService;
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

namespace ERP_API.App.Service
{
    public class AccountService : MasterService, IAccountService, IScopped
    {
        private readonly IUserService _UserService;
        private readonly PasswordHashing passwordHashing;
        private readonly Jwt jwt;

        public AccountService(DBContext context, IMapper mapper, IUserService userService, Jwt jwtService) : base(context, mapper)
        {
            _UserService = userService;
            passwordHashing = new PasswordHashing();
            jwt = jwtService;
        }

        public async Task<UserTokenDto> Login(LoginModel Model)
        {
            const string invalidCredentialsMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";

            Users? user;
            try
            {
                user = await _UserService.GetFullUser(Model.Username);
            }
            catch (KeyNotFoundException)
            {
                throw new UnauthorizedAccessException(invalidCredentialsMessage);
            }

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException(invalidCredentialsMessage);
            }

            var checkPassword = passwordHashing.VerifyPassword(Model.Password, user.HashPassword);
            if (!checkPassword)
            {
                throw new UnauthorizedAccessException(invalidCredentialsMessage);
            }

            var roleId = user.UserRoles.FirstOrDefault()?.RoleId
                ?? throw new UnauthorizedAccessException("المستخدم غير مرتبط بأي دور");
            var permisson = await _UserService.GetUserPermissions(user.Id, roleId);
            TokenResponseDto? token = await jwt.CreateTokenResponse(user, permisson);
            if (token == null)
            {
                throw new Exception("فشل في إنشاء التوكن");
            }

            // Refresh token hash + expiry already saved inside CreateTokenResponse
            user.Token = token.AccessToken;
            user.IsOnline = true;
            user.LastLogin = DateTime.UtcNow.AddHours(3);
            UserTokenDto userToken = new UserTokenDto();
            _mapper.Map<Users, UserTokenDto>(user, userToken);
            userToken.RefreshToken = token.RefreshToken;
            userToken.Token = token.AccessToken;
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
            Role role = await _UserService.GetRole(Model.Role);
            UserRoles userRoles = new UserRoles
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
                UserRoles = new List<UserRoles> { userRoles }
            };
            _context.Users.Add(users);
            await _context.SaveChangesAsync();
        }

        public async Task<UserTokenDto> RefreshToken(string refreshToken)
        {
            Users? users = await _UserService.GetUserByRefreshToken(refreshToken);
            if (users == null || !users.IsActive)
            {
                throw new UnauthorizedAccessException("انتهت صلاحية جلسة الدخول");
            }
            var roleId = users.UserRoles.FirstOrDefault()?.RoleId
                ?? throw new UnauthorizedAccessException("المستخدم غير مرتبط بأي دور");
            var permissions = await _UserService.GetUserPermissions(users.Id, roleId);
            TokenResponseDto? token = await jwt.RefreshTokensAsync(users, permissions);
            if (token == null)
            {
                throw new UnauthorizedAccessException("انتهت صلاحية جلسة الدخول");
            }

            users.Token = token.AccessToken;
            users.IsOnline = true;
            users.LastLogin = DateTime.UtcNow.AddHours(3);
            UserTokenDto userToken = new UserTokenDto();
            _mapper.Map<Users, UserTokenDto>(users, userToken);
            userToken.RefreshToken = token.RefreshToken;
            userToken.Token = token.AccessToken;
            _context.Users.Entry(users);
            await _context.SaveChangesAsync();
            return userToken;
        }

        public async Task Logout(int userId)
        {
            Users user = _UserService.GetUserById(userId);
            user.IsOnline = false;
            user.LastLogout = DateTime.UtcNow.AddHours(3);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.Token = null;
            _context.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
