using EpiManagement.Application.DTOs;
using EpiManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EpiManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EpisController : ControllerBase
{
    private readonly EpiService _service;

    public EpisController(EpiService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var e = await _service.GetByIdAsync(id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Create([FromBody] CreateEpiDto dto, CancellationToken ct)
    {
        var e = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, e);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEpiDto dto, CancellationToken ct)
    {
        try
        {
            var e = await _service.UpdateAsync(id, dto, ct);
            return Ok(e);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
