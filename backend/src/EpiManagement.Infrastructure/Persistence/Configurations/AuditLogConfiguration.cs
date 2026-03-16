using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.Action).HasColumnName("action").HasMaxLength(200).IsRequired();
        builder.Property(a => a.EntityName).HasColumnName("entity_name").HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasColumnName("entity_id");
        builder.Property(a => a.OldValues).HasColumnName("old_values");
        builder.Property(a => a.NewValues).HasColumnName("new_values");
        builder.Property(a => a.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
