using EpiManagement.Application.DTOs;
using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;

namespace EpiManagement.Application.Services;

public class EpiService
{
    private readonly IUnitOfWork _uow;

    public EpiService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<EpiDto>> GetAllAsync(CancellationToken ct = default)
    {
        var epis = await _uow.Epis.GetAllAsync(ct);
        return epis.Select(Map);
    }

    public async Task<EpiDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _uow.Epis.GetByIdAsync(id, ct);
        return e is null ? null : Map(e);
    }

    public async Task<EpiDto> CreateAsync(CreateEpiDto dto, CancellationToken ct = default)
    {
        var epi = new Epi(dto.Name, dto.Code, dto.Description, dto.ValidityDays, dto.Type);
        await _uow.Epis.AddAsync(epi, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(epi);
    }

    public async Task<EpiDto> UpdateAsync(Guid id, UpdateEpiDto dto, CancellationToken ct = default)
    {
        var epi = await _uow.Epis.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("EPI não encontrado.");
        epi.Update(dto.Name, dto.Code, dto.Description, dto.ValidityDays, dto.Type);
        await _uow.Epis.UpdateAsync(epi, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(epi);
    }

    private static EpiDto Map(Epi e) => new(e.Id, e.Name, e.Code, e.Description, e.ValidityDays, e.Type, e.IsActive);
}
