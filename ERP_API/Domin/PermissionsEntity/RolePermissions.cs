using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;

namespace ERP_API.Domin.PermissionsEntity
{
    public class RolePermissions
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }
}
