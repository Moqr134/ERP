using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.UsersEntity
{
    public class UserPermissionsMap : IEntityTypeConfiguration<UserPermissions>
    {
        public void Configure(EntityTypeBuilder<UserPermissions> builder)
        {
            builder.ToTable("UserPermissions", "dbo");
            builder.HasKey(x=>new { x.UserId, x.PermissionId });
            builder.Property(x => x.IsAllowed).IsRequired();
            builder.HasOne(x => x.Users)
                   .WithMany(x => x.UserPermissions)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Permission)
                   .WithMany(x => x.UserPermissions)
                   .HasForeignKey(x => x.PermissionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
