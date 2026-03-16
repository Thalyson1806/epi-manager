using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface ISectorRepository
{
    Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Sector>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Sector sector, CancellationToken ct = default);
    Task UpdateAsync(Sector sector, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
