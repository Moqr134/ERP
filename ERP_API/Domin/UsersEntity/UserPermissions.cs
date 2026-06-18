using ERP_API.Domin.PermartionEntity;

namespace ERP_API.Domin.UsersEntity
{
    public class UserPermissions
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public bool IsAllowed { get; set; }
        public Users Users { get; set; }
        public Permission Permission { get; set; }
    }
}
