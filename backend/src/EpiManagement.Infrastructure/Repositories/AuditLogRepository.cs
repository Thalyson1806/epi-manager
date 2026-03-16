using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _ctx;

    public AuditLogRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.AuditLogs.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await _ctx.AuditLogs.AddAsync(log, ct);
}
