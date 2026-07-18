using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.PurchaseEntity
{
    public class PurchaseReceiptMap : IEntityTypeConfiguration<PurchaseReceipt>
    {
        public void Configure(EntityTypeBuilder<PurchaseReceipt> builder)
        {
            builder.ToTable("PurchaseReceipts", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ReceiptNumber).IsRequired().HasMaxLength(40);
            builder.HasIndex(x => x.ReceiptNumber).IsUnique();
            builder.Property(x => x.SupplierId).IsRequired();
            builder.Property(x => x.SupplierInvoiceNumber).HasMaxLength(40);
            builder.Property(x => x.SubTotal).IsRequired();
            builder.Property(x => x.Discount).IsRequired();
            builder.Property(x => x.Total).IsRequired();
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
            builder.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Lines).WithOne(x => x.PurchaseReceipt).HasForeignKey(x => x.PurchaseReceiptId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
