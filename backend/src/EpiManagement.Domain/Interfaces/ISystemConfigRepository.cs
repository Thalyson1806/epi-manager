using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface ISystemConfigRepository
{
    Task<SystemConfig?> GetAsync(CancellationToken ct = default);
    Task SaveAsync(SystemConfig config, CancellationToken ct = default);
}
