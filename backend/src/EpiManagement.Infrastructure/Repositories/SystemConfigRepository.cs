using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class SystemConfigRepository : ISystemConfigRepository
{
    private readonly AppDbContext _ctx;
    public SystemConfigRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<SystemConfig?> GetAsync(CancellationToken ct = default)
        => await _ctx.SystemConfigs.FirstOrDefaultAsync(ct);

    public async Task SaveAsync(SystemConfig config, CancellationToken ct = default)
    {
        var existing = await _ctx.SystemConfigs.FirstOrDefaultAsync(ct);
        if (existing is null)
            _ctx.SystemConfigs.Add(config);
        else
        {
            existing.SmtpHost = config.SmtpHost;
            existing.SmtpPort = config.SmtpPort;
            existing.SmtpUser = config.SmtpUser;
            existing.SmtpPassword = config.SmtpPassword;
            existing.SmtpFromEmail = config.SmtpFromEmail;
            existing.SmtpFromName = config.SmtpFromName;
            existing.SmtpUseSsl = config.SmtpUseSsl;
            existing.AlertEnabled = config.AlertEnabled;
            existing.AlertHour = config.AlertHour;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
