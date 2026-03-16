using EpiManagement.Application.DTOs;
using EpiManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EpiManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SectorsController : ControllerBase
{
    private readonly SectorService _service;

    public SectorsController(SectorService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var s = await _service.GetByIdAsync(id, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Create([FromBody] CreateSectorDto dto, CancellationToken ct)
    {
        var s = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSectorDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateAsync(id, dto, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    // ── EPIs do setor ────────────────────────────────────────────────────────

    [HttpGet("{sectorId:guid}/epis")]
    public async Task<IActionResult> GetEpis(Guid sectorId, CancellationToken ct)
    {
        try { return Ok(await _service.GetEpisAsync(sectorId, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{sectorId:guid}/epis")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> AddEpi(Guid sectorId, [FromBody] CreateSectorEpiDto dto, CancellationToken ct)
    {
        try
        {
            var result = await _service.AddEpiAsync(sectorId, new CreateSectorEpiDto(
                sectorId, dto.EpiId, dto.IsRequired, dto.ReplacementPeriodDays, dto.MaxQuantityAllowed), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
    }

    [HttpPut("epis/{sectorEpiId:guid}")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> UpdateEpi(Guid sectorEpiId, [FromBody] UpdateSectorEpiDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateEpiAsync(sectorEpiId, dto, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("epis/{sectorEpiId:guid}")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> RemoveEpi(Guid sectorEpiId, CancellationToken ct)
    {
        try { await _service.RemoveEpiAsync(sectorEpiId, ct); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("suggested-epis/{employeeId:guid}")]
    public async Task<IActionResult> GetSuggestedEpis(Guid employeeId, CancellationToken ct)
    {
        try { return Ok(await _service.GetSuggestedEpisForEmployeeAsync(employeeId, ct)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
