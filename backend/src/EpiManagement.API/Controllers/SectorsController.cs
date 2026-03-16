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
        try
        {
            var s = await _service.UpdateAsync(id, dto, ct);
            return Ok(s);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
