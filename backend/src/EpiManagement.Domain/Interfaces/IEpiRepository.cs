using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface IEpiRepository
{
    Task<Epi?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Epi>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Epi>> GetBySectorAsync(Guid sectorId, CancellationToken ct = default);
    Task AddAsync(Epi epi, CancellationToken ct = default);
    Task UpdateAsync(Epi epi, CancellationToken ct = default);
}
