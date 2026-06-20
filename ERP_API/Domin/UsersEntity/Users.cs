using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;

namespace ERP_API.Domin.UsersEntity
{
    public class Users
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string HashPassword { get; set; }
        public required string Email { get; set; }
        public bool IsOnline { get; set; } = false;
        public bool IsRemoved { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow.AddHours(3);
        public int CreateUserId { get; set; }
        public DateTime? RemoveDate { get; set; }
        public int RemoveUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int UpdateUserId { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastLogout { get; set; }
        public bool IsActive { get; set; }
        public byte[]? Version { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; } = null;
        public UserRoles UserRoles { get; set; }
        public ICollection<UserPermissions> UserPermissions { get; set; }
    }
}
