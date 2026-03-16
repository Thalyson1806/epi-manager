using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class SectorEpiConfiguration : IEntityTypeConfiguration<SectorEpi>
{
    public void Configure(EntityTypeBuilder<SectorEpi> builder)
    {
        builder.ToTable("sector_epis");
        builder.HasKey(se => se.Id);
        builder.Property(se => se.Id).HasColumnName("id");
        builder.Property(se => se.SectorId).HasColumnName("sector_id").IsRequired();
        builder.Property(se => se.EpiId).HasColumnName("epi_id").IsRequired();
        builder.Property(se => se.IsRequired).HasColumnName("is_required").IsRequired();
        builder.Property(se => se.ReplacementPeriodDays).HasColumnName("replacement_period_days").IsRequired();
        builder.Property(se => se.MaxQuantityAllowed).HasColumnName("max_quantity_allowed").IsRequired();

        builder.HasOne(se => se.Sector).WithMany(s => s.SectorEpis).HasForeignKey(se => se.SectorId);
        builder.HasOne(se => se.Epi).WithMany(e => e.SectorEpis).HasForeignKey(se => se.EpiId);
    }
}
