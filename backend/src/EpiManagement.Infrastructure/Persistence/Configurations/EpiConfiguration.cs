using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class EpiConfiguration : IEntityTypeConfiguration<Epi>
{
    public void Configure(EntityTypeBuilder<Epi> builder)
    {
        builder.ToTable("epis");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(e => e.ValidityDays).HasColumnName("validity_days").IsRequired();
        builder.Property(e => e.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(e => e.Code).IsUnique();
    }
}
