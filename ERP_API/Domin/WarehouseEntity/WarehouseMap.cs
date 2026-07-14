using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.WarehouseEntity
{
    public class WarehouseMap : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.Property(x => x.Location).HasMaxLength(200);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(250);
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.UpdateUserId);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.RemoveDate);
            builder.Property(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.HasQueryFilter(x => x.IsRemoved == false);
        }
    }
}
