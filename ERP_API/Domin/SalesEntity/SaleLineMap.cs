using ERP_API.Domin.ProductEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.SalesEntity
{
    public class SaleLineMap : IEntityTypeConfiguration<SaleLine>
    {
        public void Configure(EntityTypeBuilder<SaleLine> builder)
        {
            builder.ToTable("SaleLines", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SaleId).IsRequired();
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.ProductName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Barcode).HasMaxLength(50);
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.UnitPrice).IsRequired();
            builder.Property(x => x.LineTotal).IsRequired();
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Ignore(x => x.UpdateDate);
            builder.Ignore(x => x.UpdateUserId);
            builder.Ignore(x => x.IsRemoved);
            builder.Ignore(x => x.RemoveDate);
            builder.Ignore(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
