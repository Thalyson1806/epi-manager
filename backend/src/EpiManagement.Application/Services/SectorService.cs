using EpiManagement.Application.DTOs;
using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;

namespace EpiManagement.Application.Services;

public class SectorService
{
    private readonly IUnitOfWork _uow;

    public SectorService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SectorDto>> GetAllAsync(CancellationToken ct = default)
    {
        var sectors = await _uow.Sectors.GetAllAsync(ct);
        return sectors.Select(s => new SectorDto(s.Id, s.Name, s.Description, s.Employees.Count));
    }

    public async Task<SectorDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _uow.Sectors.GetByIdAsync(id, ct);
        return s is null ? null : new SectorDto(s.Id, s.Name, s.Description, s.Employees.Count);
    }

    public async Task<SectorDto> CreateAsync(CreateSectorDto dto, CancellationToken ct = default)
    {
        var sector = new Sector(dto.Name, dto.Description);
        await _uow.Sectors.AddAsync(sector, ct);
        await _uow.SaveChangesAsync(ct);
        return new SectorDto(sector.Id, sector.Name, sector.Description, 0);
    }

    public async Task<SectorDto> UpdateAsync(Guid id, UpdateSectorDto dto, CancellationToken ct = default)
    {
        var s = await _uow.Sectors.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Setor não encontrado.");
        s.Update(dto.Name, dto.Description);
        await _uow.Sectors.UpdateAsync(s, ct);
        await _uow.SaveChangesAsync(ct);
        return new SectorDto(s.Id, s.Name, s.Description, s.Employees.Count);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _uow.Sectors.DeleteAsync(id, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── EPIs vinculados ao setor ─────────────────────────────────────────────

    public async Task<IEnumerable<SectorEpiDto>> GetEpisAsync(Guid sectorId, CancellationToken ct = default)
    {
        var sector = await _uow.Sectors.GetByIdAsync(sectorId, ct)
            ?? throw new KeyNotFoundException("Setor não encontrado.");
        return sector.SectorEpis.Select(MapEpi);
    }

    public async Task<SectorEpiDto> AddEpiAsync(Guid sectorId, CreateSectorEpiDto dto, CancellationToken ct = default)
    {
        var sector = await _uow.Sectors.GetByIdAsync(sectorId, ct)
            ?? throw new KeyNotFoundException("Setor não encontrado.");

        var epi = await _uow.Epis.GetByIdAsync(dto.EpiId, ct)
            ?? throw new KeyNotFoundException("EPI não encontrado.");

        if (sector.SectorEpis.Any(se => se.EpiId == dto.EpiId))
            throw new InvalidOperationException("EPI já vinculado a este setor.");

        var sectorEpi = new SectorEpi(sectorId, dto.EpiId, dto.IsRequired, dto.ReplacementPeriodDays, dto.MaxQuantityAllowed);
        await _uow.Sectors.AddSectorEpiAsync(sectorEpi, ct);
        await _uow.SaveChangesAsync(ct);

        return new SectorEpiDto(sectorEpi.Id, sectorId, sector.Name, dto.EpiId, epi.Name,
            dto.IsRequired, dto.ReplacementPeriodDays, dto.MaxQuantityAllowed);
    }

    public async Task<SectorEpiDto> UpdateEpiAsync(Guid sectorEpiId, UpdateSectorEpiDto dto, CancellationToken ct = default)
    {
        var se = await _uow.Sectors.GetSectorEpiAsync(sectorEpiId, ct)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");
        se.Update(dto.IsRequired, dto.ReplacementPeriodDays, dto.MaxQuantityAllowed);
        await _uow.Sectors.UpdateSectorEpiAsync(se, ct);
        await _uow.SaveChangesAsync(ct);
        return MapEpi(se);
    }

    public async Task RemoveEpiAsync(Guid sectorEpiId, CancellationToken ct = default)
    {
        var se = await _uow.Sectors.GetSectorEpiAsync(sectorEpiId, ct)
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");
        await _uow.Sectors.DeleteSectorEpiAsync(se, ct);
        await _uow.SaveChangesAsync(ct);
    }

    /// <summary>EPIs do setor do funcionário para pré-popular a entrega.</summary>
    public async Task<IEnumerable<SectorEpiDto>> GetSuggestedEpisForEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(employeeId, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        if (emp.SectorId == Guid.Empty)
            return Enumerable.Empty<SectorEpiDto>();

        var sector = await _uow.Sectors.GetByIdAsync(emp.SectorId, ct);
        if (sector is null) return Enumerable.Empty<SectorEpiDto>();

        return sector.SectorEpis.Select(MapEpi);
    }

    private static SectorEpiDto MapEpi(SectorEpi se) => new(
        se.Id, se.SectorId, se.Sector?.Name ?? string.Empty,
        se.EpiId, se.Epi?.Name ?? string.Empty,
        se.IsRequired, se.ReplacementPeriodDays, se.MaxQuantityAllowed
    );
}
