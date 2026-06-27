using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.RoleEntity
{
    public class RoleMap : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role","dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(150);
            builder.Property(x => x.CreateDate);
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.RemoveDate);
            builder.Property(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();

            builder.HasQueryFilter(x => x.IsRemoved == false);
        }
    }
}
