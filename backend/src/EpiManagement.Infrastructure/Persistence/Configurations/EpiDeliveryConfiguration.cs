using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class EpiDeliveryConfiguration : IEntityTypeConfiguration<EpiDelivery>
{
    public void Configure(EntityTypeBuilder<EpiDelivery> builder)
    {
        builder.ToTable("epi_deliveries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(d => d.OperatorId).HasColumnName("operator_id").IsRequired();
        builder.Property(d => d.DeliveryDate).HasColumnName("delivery_date").IsRequired();
        builder.Property(d => d.BiometricSignature).HasColumnName("biometric_signature");
        builder.Property(d => d.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(d => d.Employee).WithMany(e => e.EpiDeliveries).HasForeignKey(d => d.EmployeeId);
        builder.HasOne(d => d.Operator).WithMany(u => u.Deliveries).HasForeignKey(d => d.OperatorId);
    }
}
