using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class EpiRepository : IEpiRepository
{
    private readonly AppDbContext _ctx;

    public EpiRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Epi?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Epis.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IEnumerable<Epi>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Epis.ToListAsync(ct);

    public async Task<IEnumerable<Epi>> GetBySectorAsync(Guid sectorId, CancellationToken ct = default)
        => await _ctx.SectorEpis.Include(se => se.Epi)
            .Where(se => se.SectorId == sectorId).Select(se => se.Epi).ToListAsync(ct);

    public async Task AddAsync(Epi epi, CancellationToken ct = default)
        => await _ctx.Epis.AddAsync(epi, ct);

    public Task UpdateAsync(Epi epi, CancellationToken ct = default)
    {
        _ctx.Epis.Update(epi);
        return Task.CompletedTask;
    }
}
