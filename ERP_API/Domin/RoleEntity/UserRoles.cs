using ERP_API.Domin.UsersEntity;

namespace ERP_API.Domin.RoleEntity
{
    public class UserRoles
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public Users Users { get; set; }
        public Role Role { get; set; }
    }
}
