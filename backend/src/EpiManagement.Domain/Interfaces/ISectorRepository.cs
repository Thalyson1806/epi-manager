using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface ISectorRepository
{
    Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Sector>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Sector sector, CancellationToken ct = default);
    Task UpdateAsync(Sector sector, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<SectorEpi?> GetSectorEpiAsync(Guid sectorEpiId, CancellationToken ct = default);
    Task AddSectorEpiAsync(SectorEpi sectorEpi, CancellationToken ct = default);
    Task UpdateSectorEpiAsync(SectorEpi sectorEpi, CancellationToken ct = default);
    Task DeleteSectorEpiAsync(SectorEpi sectorEpi, CancellationToken ct = default);
}
