using ERP_API.Domin.PermissionsEntity;
using ERP_API.Domin.UsersEntity;

namespace ERP_API.Domin.PermartionEntity
{
    public class Permission
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public Users? User { get; set; }
        public ICollection<RolePermissions> RolePermissions { get; set; } 
        public ICollection<UserPermissions> UserPermissions { get; set; }
    }
}
