using EpiManagement.Application.DTOs;
using EpiManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EpiManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _service;

    public EmployeesController(EmployeeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var emp = await _service.GetByIdAsync(id, ct);
        return emp is null ? NotFound() : Ok(emp);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto, CancellationToken ct)
    {
        try
        {
            var emp = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = emp.Id }, emp);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken ct)
    {
        try
        {
            var emp = await _service.UpdateAsync(id, dto, ct);
            return Ok(emp);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/biometric")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> SetBiometric(Guid id, [FromBody] EmployeeBiometricDto dto, CancellationToken ct)
    {
        try
        {
            await _service.SetBiometricAsync(id, dto.TemplateBase64, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _service.ActivateAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _service.DeactivateAsync(id, ct);
        return NoContent();
    }
}
