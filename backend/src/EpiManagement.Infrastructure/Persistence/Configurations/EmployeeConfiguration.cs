using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Cpf).HasColumnName("cpf").HasMaxLength(14).IsRequired();
        builder.Property(e => e.Registration).HasColumnName("registration").HasMaxLength(50).IsRequired();
        builder.Property(e => e.SectorId).HasColumnName("sector_id").IsRequired();
        builder.Property(e => e.Position).HasColumnName("position").HasMaxLength(200).IsRequired();
        builder.Property(e => e.AdmissionDate).HasColumnName("admission_date").IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").IsRequired();
        builder.Property(e => e.BiometricTemplate).HasColumnName("biometric_template");
        builder.Property(e => e.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(e => e.Cpf).IsUnique();
        builder.HasIndex(e => e.Registration).IsUnique();

        builder.HasOne(e => e.Sector)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.SectorId);
    }
}
