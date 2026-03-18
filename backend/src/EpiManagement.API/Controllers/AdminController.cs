using EpiManagement.Application.DTOs;
using EpiManagement.Application.Services;
using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EpiManagement.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly AlertService _alertService;
    private readonly EmailService _emailService;

    public AdminController(IUnitOfWork uow, AlertService alertService, EmailService emailService)
    {
        _uow = uow;
        _alertService = alertService;
        _emailService = emailService;
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var config = await _uow.SystemConfig.GetAsync(ct) ?? new SystemConfig();
        return Ok(new SmtpConfigDto(
            config.SmtpHost, config.SmtpPort, config.SmtpUser,
            string.IsNullOrEmpty(config.SmtpPassword) ? "" : "••••••••",
            config.SmtpFromEmail, config.SmtpFromName,
            config.SmtpUseSsl, config.AlertEnabled, config.AlertHour));
    }

    [HttpPost("config")]
    public async Task<IActionResult> SaveConfig([FromBody] SmtpConfigDto dto, CancellationToken ct)
    {
        var config = await _uow.SystemConfig.GetAsync(ct) ?? new SystemConfig();
        config.SmtpHost = dto.SmtpHost;
        config.SmtpPort = dto.SmtpPort;
        config.SmtpUser = dto.SmtpUser;
        if (dto.SmtpPassword != "••••••••")
            config.SmtpPassword = dto.SmtpPassword;
        config.SmtpFromEmail = dto.SmtpFromEmail;
        config.SmtpFromName = dto.SmtpFromName;
        config.SmtpUseSsl = dto.SmtpUseSsl;
        config.AlertEnabled = dto.AlertEnabled;
        config.AlertHour = dto.AlertHour;
        config.UpdatedAt = DateTime.UtcNow;
        await _uow.SystemConfig.SaveAsync(config, ct);
        await _uow.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("config/test-email")]
    public async Task<IActionResult> TestEmail(CancellationToken ct)
    {
        var config = await _uow.SystemConfig.GetAsync(ct);
        if (config is null || string.IsNullOrEmpty(config.SmtpHost))
            return BadRequest("Configuração SMTP não encontrada.");
        try
        {
            await _emailService.SendAsync(config,
                config.SmtpFromEmail, "Teste",
                "Digital-RH — Teste de e-mail",
                "<h3>✅ E-mail de teste enviado com sucesso!</h3><p>A configuração SMTP está funcionando.</p>",
                ct);
            return Ok(new { message = "E-mail de teste enviado com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Erro: {ex.Message}" });
        }
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var items = await _alertService.GetOverdueItemsAsync(ct);
        return Ok(items);
    }

    [HttpPost("send-alerts")]
    public async Task<IActionResult> SendAlerts(CancellationToken ct)
    {
        await _alertService.SendOverdueAlertsAsync(ct);
        return Ok(new { message = "Alertas enviados." });
    }
}
