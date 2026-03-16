using EpiManagement.Application.DTOs;
using EpiManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EpiManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly EpiDeliveryService _service;
    private readonly PdfService _pdf;

    public DeliveriesController(EpiDeliveryService service, PdfService pdf)
    {
        _service = service;
        _pdf = pdf;
    }

    [HttpPost("identify")]
    public async Task<IActionResult> IdentifyByBiometric([FromBody] BiometricIdentifyDto dto, CancellationToken ct)
    {
        var result = await _service.IdentifyByBiometricAsync(dto.BiometricSampleBase64, ct);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await _service.GetDashboardAsync(ct));

    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetByEmployee(Guid employeeId, CancellationToken ct)
        => Ok(await _service.GetByEmployeeAsync(employeeId, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var d = await _service.GetByIdAsync(id, ct);
        return d is null ? NotFound() : Ok(d);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Warehouse")]
    public async Task<IActionResult> Create([FromBody] CreateEpiDeliveryDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var operatorId))
            return Unauthorized();

        try
        {
            var d = await _service.CreateDeliveryAsync(dto, operatorId, ct);
            return CreatedAtAction(nameof(GetById), new { id = d.Id }, d);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("employee/{employeeId:guid}/pdf")]
    public async Task<IActionResult> ExportPdf(
        Guid employeeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct)
    {
        try
        {
            var pdf = await _pdf.GenerateEmployeeEpiCardAsync(employeeId, startDate, endDate, ct);
            return File(pdf, "application/pdf", $"ficha-epi-{employeeId}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
