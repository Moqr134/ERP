using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domin.SuppliersEntity
{
    public class SuppliersMap : IEntityTypeConfiguration<Suppliers>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Suppliers> builder)
        {
            builder.ToTable("Suppliers", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreateDate).IsRequired();
            builder.Property(x => x.CreateUserId);
            builder.Property(x => x.UpdateDate);
            builder.Property(x => x.UpdateUserId);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.RemoveDate);
            builder.Property(x => x.RemoveUserId);
            builder.Property(x => x.Version).IsRowVersion();
            builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ContactName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.PhoneNumper).IsRequired().HasMaxLength(20);

            builder.HasQueryFilter(x => x.IsRemoved == false);
        }
    }
}
