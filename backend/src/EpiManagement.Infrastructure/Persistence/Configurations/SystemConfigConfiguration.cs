using EpiManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EpiManagement.Infrastructure.Persistence.Configurations;

public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> builder)
    {
        builder.ToTable("system_config");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.SmtpHost).HasColumnName("smtp_host").HasMaxLength(200);
        builder.Property(s => s.SmtpPort).HasColumnName("smtp_port");
        builder.Property(s => s.SmtpUser).HasColumnName("smtp_user").HasMaxLength(200);
        builder.Property(s => s.SmtpPassword).HasColumnName("smtp_password").HasMaxLength(200);
        builder.Property(s => s.SmtpFromEmail).HasColumnName("smtp_from_email").HasMaxLength(200);
        builder.Property(s => s.SmtpFromName).HasColumnName("smtp_from_name").HasMaxLength(200);
        builder.Property(s => s.SmtpUseSsl).HasColumnName("smtp_use_ssl");
        builder.Property(s => s.AlertEnabled).HasColumnName("alert_enabled");
        builder.Property(s => s.AlertHour).HasColumnName("alert_hour");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
    }
}
