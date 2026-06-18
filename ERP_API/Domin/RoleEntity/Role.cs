using Common.App;
using ERP_API.Domin.PermissionsEntity;

namespace ERP_API.Domin.RoleEntity
{
    public class Role : Entity
    {
        public string Name { get; set; } 
        public string Description { get; set; }
        public ICollection<RolePermissions> RolePermissions { get; set; }
        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
