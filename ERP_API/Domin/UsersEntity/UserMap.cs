using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domin.UsersEntity
{
    public class UserMap : IEntityTypeConfiguration<Users>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Users> builder)
        {
            builder.ToTable("Users", "dbo");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username).HasMaxLength(16);
            builder.Property(x => x.HashPassword);
            builder.Property(x => x.IsOnline);
            builder.Property(x => x.LastLogin);
            builder.Property(x => x.LastLogout);
            builder.Property(x => x.IsActive);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.RemoveDate);
            builder.Property(x => x.RemoveUserId);
            builder.Property(x => x.UpdateUserId);
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.CreateDate);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.Version).IsRowVersion();
            builder.Property(x => x.RefreshToken);
            builder.Property(x => x.RefreshTokenExpiryTime);
            builder.Ignore(x => x.Token);
        }
    }
}
