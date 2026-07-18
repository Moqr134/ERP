using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.SalesEntity
{
    public class SaleMap : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.ToTable("Sales", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(40);
            builder.HasIndex(x => x.InvoiceNumber).IsUnique();
            builder.Property(x => x.PaymentMethod).IsRequired().HasMaxLength(20);
            builder.Property(x => x.SubTotal).IsRequired();
            builder.Property(x => x.Discount).IsRequired();
            builder.Property(x => x.Total).IsRequired();
            builder.Property(x => x.PaidAmount).IsRequired();
            builder.Property(x => x.ChangeAmount).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(250);
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.WarehouseId).IsRequired();
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.UpdateUserId);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.RemoveDate);
            builder.Property(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.HasQueryFilter(x => x.IsRemoved == false);
            builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Lines).WithOne(x => x.Sale).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
