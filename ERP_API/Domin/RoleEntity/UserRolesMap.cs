using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domin.RoleEntity
{
    public class UserRolesMap : IEntityTypeConfiguration<UserRoles>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserRoles> builder)
        {
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });
            builder.HasOne(ur => ur.Users)
                .WithOne(u => u.UserRoles)
                .HasForeignKey<UserRoles>(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
