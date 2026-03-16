using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class SectorRepository : ISectorRepository
{
    private readonly AppDbContext _ctx;

    public SectorRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Sectors.Include(s => s.Employees).Include(s => s.SectorEpis).ThenInclude(se => se.Epi)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IEnumerable<Sector>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Sectors.Include(s => s.Employees).ToListAsync(ct);

    public async Task AddAsync(Sector sector, CancellationToken ct = default)
        => await _ctx.Sectors.AddAsync(sector, ct);

    public Task UpdateAsync(Sector sector, CancellationToken ct = default)
    {
        _ctx.Sectors.Update(sector);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _ctx.Sectors.FindAsync(new object[] { id }, ct);
        if (s is not null) _ctx.Sectors.Remove(s);
    }

    public async Task<SectorEpi?> GetSectorEpiAsync(Guid sectorEpiId, CancellationToken ct = default)
        => await _ctx.SectorEpis.Include(se => se.Epi).FirstOrDefaultAsync(se => se.Id == sectorEpiId, ct);

    public async Task AddSectorEpiAsync(SectorEpi sectorEpi, CancellationToken ct = default)
        => await _ctx.SectorEpis.AddAsync(sectorEpi, ct);

    public Task UpdateSectorEpiAsync(SectorEpi sectorEpi, CancellationToken ct = default)
    {
        _ctx.SectorEpis.Update(sectorEpi);
        return Task.CompletedTask;
    }

    public Task DeleteSectorEpiAsync(SectorEpi sectorEpi, CancellationToken ct = default)
    {
        _ctx.SectorEpis.Remove(sectorEpi);
        return Task.CompletedTask;
    }
}
