using Domin.TokenDto;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.UsersEntity;
using ERPDto.UserDto;
using SherdProject.DTO;

namespace ERP_API.App.IService
{
    public interface IAccountService
    {
        public Task<UserTokenDto> Login(LoginModel Model);
        public Task Register(RegisterModel Model, int userId);
        public Task<UserTokenDto> RefreshToken(string refreshToken);
        public Task Logout(int userId);
    }
}
