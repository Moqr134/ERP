using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.UsersEntity;
using ERPDto.PaigingDto;
using ERPDto.UserDto;

namespace ERP_API.App.IService
{
    public interface IUserService
    {
        public UserOut GetUser(int id);
        public Task<List<UserOut>> GetUsers(PageDto pageDto);
        public Task<UserOut> CheckUser(string Name);
        public Task<Users?> GetFullUser(string Name);
        public Task<Users?> GetUserByRefreshToken(string refreshToken);
        public Task<Users?> CheckUserExsist(string Name);
        public Task<Role?> GetUserRole(int userId);
        public Task<Role> GetRole(string RoleName);
        public Task<List<Permission>> GetRolePermissions(int Id);
        public Task<List<Role>> GetUserRoles(int userId);
        public Task<List<Permission>> GetUserPermissions(int userId);
    }
}
