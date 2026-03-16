using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(AuditLog log, CancellationToken ct = default);
}
