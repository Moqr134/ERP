using ERP_API.Domin.UsersEntity;

namespace ERP_API.Domin.PermartionEntity
{
    public class Permation
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }
        public Users? User { get; set; }
    }
}
