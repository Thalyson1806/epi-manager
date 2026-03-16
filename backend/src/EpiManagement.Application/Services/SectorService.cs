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
}
