using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;
using ERPDto.RolesDto;
using ERPDto.UserDto;

namespace ERP_API.App.IService
{
    public interface IRoleService
    {
        public Task<List<RoleDto>> GetAllRoles();
        public Task CreateRole(RoleDto roleDto, int createUserId);
        public Task UpdateRole(int UpdateUserId, RoleDto roleDto);
        public Task DeleteRole(int id, int deleteUserId);
        public Task<Role?> GetRoleById(int id);
        public Task CreateRolePermission(int roleId, List<int> permissionIds);
        public Task<List<PermissionDto>> GetAllPermissions();
        public Task<RolePermissionViewDto> GetRolePermissions(int roleId);
    }
}
