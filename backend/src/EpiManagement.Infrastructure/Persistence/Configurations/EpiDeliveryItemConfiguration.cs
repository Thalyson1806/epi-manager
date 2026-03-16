using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class EpiDeliveryItemConfiguration : IEntityTypeConfiguration<EpiDeliveryItem>
{
    public void Configure(EntityTypeBuilder<EpiDeliveryItem> builder)
    {
        builder.ToTable("epi_delivery_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.EpiDeliveryId).HasColumnName("epi_delivery_id").IsRequired();
        builder.Property(i => i.EpiId).HasColumnName("epi_id").IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(i => i.NextReplacementDate).HasColumnName("next_replacement_date").IsRequired();

        builder.HasOne(i => i.EpiDelivery).WithMany(d => d.Items).HasForeignKey(i => i.EpiDeliveryId);
        builder.HasOne(i => i.Epi).WithMany(e => e.DeliveryItems).HasForeignKey(i => i.EpiId);
    }
}
