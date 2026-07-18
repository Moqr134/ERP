using ERP_API.Domin.ProductEntity;
using ERP_API.Domin.WarehouseEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.StockTransactionsEntity
{
    public class StockTransactionsMap : IEntityTypeConfiguration<StockTransactions>
    {
        public void Configure(EntityTypeBuilder<StockTransactions> builder)
        {
            builder.ToTable("StockTransactions", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.UpdateUserId);
            builder.Ignore(x => x.IsRemoved);
            builder.Ignore(x => x.RemoveDate);
            builder.Ignore(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.WarehouseId).IsRequired();
            builder.Property(x => x.RelatedWarehouseId);
            builder.Property(x => x.TransactionType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.ReferenceId).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(250);
            builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.RelatedWarehouseId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
