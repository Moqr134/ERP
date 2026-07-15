using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.StockTransferEntity
{
    public class StockTransferLineMap : IEntityTypeConfiguration<StockTransferLine>
    {
        public void Configure(EntityTypeBuilder<StockTransferLine> builder)
        {
            builder.ToTable("StockTransferLines", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.StockTransferId).IsRequired();
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.UpdateUserId);
            builder.Ignore(x => x.IsRemoved);
            builder.Ignore(x => x.RemoveDate);
            builder.Ignore(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
