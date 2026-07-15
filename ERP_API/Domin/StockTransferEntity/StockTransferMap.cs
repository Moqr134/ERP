using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.StockTransferEntity
{
    public class StockTransferMap : IEntityTypeConfiguration<StockTransfer>
    {
        public void Configure(EntityTypeBuilder<StockTransfer> builder)
        {
            builder.ToTable("StockTransfers", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TransferNumber).IsRequired().HasMaxLength(40);
            builder.HasIndex(x => x.TransferNumber).IsUnique();
            builder.Property(x => x.FromWarehouseId).IsRequired();
            builder.Property(x => x.ToWarehouseId).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
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
            builder.HasOne(x => x.FromWarehouse).WithMany().HasForeignKey(x => x.FromWarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToWarehouse).WithMany().HasForeignKey(x => x.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Lines).WithOne(x => x.StockTransfer).HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
