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
        public Users GetUserById(int Id);
        public Task<List<UserOut>> GetUsers(PageDto pageDto);
        public Task<UsersListResponse> GetUsersPaged(PageDto pageDto);
        public Task<UserDetailDto> GetUserDetail(int id);
        public Task<UsersInfo> GetUsersInfo();
        public Task CreateUser(CreateUserModel model, int createUserId);
        public Task SetUserActive(int userId, bool isActive, int updateUserId);
        public Task AssignUserRoles(int userId, List<int> roleIds, int updateUserId);
        public Task ChangePassword(int userId, string newPassword, int updateUserId);
        public Task ChangeMyPassword(int userId, string currentPassword, string newPassword);
        public Task<List<UserPermissionViewDto>> GetUserPermissionsView(int userId);
        public Task<UserOut> CheckUser(string Name);
        public Task<Users?> GetFullUser(string Name);
        public Task<Users?> GetUserByRefreshToken(string refreshToken);
        public Task<Users?> CheckUserExsist(string Name);
        public Task<Role?> GetUserRole(int userId);
        public Task<Role> GetRole(string RoleName);
        public Task<Role> GetRole(int RoleId);
        public Task<List<Permission>> GetRolePermissions(int Id);
        public Task<List<Role>> GetUserRoles(int userId);
        public Task<List<Permission>> GetUserPermissions(int userId, int roleId);
        public Task<List<Permission>> GetEffectivePermissions(int userId);
        public Task UpdateUser(UpdateUserModel model, int updateUserId);
        public Task DeleteUser(int userId, int removeUserId);
        public Task UpdateUserpermission(int userId, List<UserPermissionDto> userPermission);
    }
}
