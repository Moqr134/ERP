using ERP_API.Domin.UsersEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP_API.Domin.PermartionEntity
{
    public class PermationMap : IEntityTypeConfiguration<Permation>
    {
        public void Configure(EntityTypeBuilder<Permation> builder)
        {
            builder.ToTable("Permation", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(20);
            builder.Property(x => x.UserId);
            builder.HasOne(s => s.User)
                .WithMany(P => P.Permations)
                .HasForeignKey(x => x.UserId)
                .HasForeignKey(x=>x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
