using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.ProductEntity
{
    public class ProductUnitMap : IEntityTypeConfiguration<ProductUnit>
    {
        public void Configure(EntityTypeBuilder<ProductUnit> builder)
        {
            builder.ToTable("ProductUnits", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Factor).IsRequired();
            builder.Property(x => x.SellingPrice).IsRequired();
            builder.Property(x => x.IsBase).IsRequired();
            builder.Property(x => x.IsDefaultForSale).IsRequired();
            builder.Property(x => x.SortOrder).IsRequired();
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.UpdateUserId);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.RemoveDate);
            builder.Property(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.HasQueryFilter(x => x.IsRemoved == false);
            builder.HasOne(x => x.Product)
                .WithMany(x => x.Units)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(x => new { x.ProductId, x.Name });
        }
    }
}
