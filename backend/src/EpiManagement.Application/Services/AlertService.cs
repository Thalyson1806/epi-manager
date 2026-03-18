using EpiManagement.Application.DTOs;
using EpiManagement.Domain.Interfaces;

namespace EpiManagement.Application.Services;

public class AlertService
{
    private readonly IUnitOfWork _uow;
    private readonly EmailService _email;

    public AlertService(IUnitOfWork uow, EmailService email)
    {
        _uow = uow;
        _email = email;
    }

    public async Task<IEnumerable<OverdueEpiItemDto>> GetOverdueItemsAsync(CancellationToken ct = default)
    {
        var items = await _uow.EpiDeliveries.GetOverdueAsync(ct);
        var now = DateTime.UtcNow;
        return items.Select(i => new OverdueEpiItemDto(
            i.EpiDelivery.EmployeeId,
            i.EpiDelivery.Employee?.Name ?? string.Empty,
            i.EpiDelivery.Employee?.Registration ?? string.Empty,
            i.EpiDelivery.Employee?.WorkShift,
            i.EpiDelivery.Employee?.Position ?? string.Empty,
            i.EpiDelivery.Employee?.Sector?.Name ?? string.Empty,
            i.EpiDelivery.Employee?.Sector?.SupervisorEmail,
            i.EpiId,
            i.Epi?.Name ?? string.Empty,
            i.Epi?.Code ?? string.Empty,
            i.NextReplacementDate,
            (int)(now - i.NextReplacementDate).TotalDays
        ));
    }

    public async Task SendOverdueAlertsAsync(CancellationToken ct = default)
    {
        var config = await _uow.SystemConfig.GetAsync(ct);
        if (config is null || !config.AlertEnabled || string.IsNullOrEmpty(config.SmtpHost))
            return;

        var items = (await GetOverdueItemsAsync(ct)).ToList();
        if (!items.Any()) return;

        // Group by sector supervisor email
        var bySector = items
            .Where(i => !string.IsNullOrEmpty(i.SectorSupervisorEmail))
            .GroupBy(i => new { i.SectorName, i.SectorSupervisorEmail });

        foreach (var group in bySector)
        {
            var html = BuildEmailHtml(group.Key.SectorName, group.ToList());
            await _email.SendAsync(config,
                group.Key.SectorSupervisorEmail!,
                $"Encarregado — {group.Key.SectorName}",
                $"⚠️ EPIs vencidos — {group.Key.SectorName} — {DateTime.Now:dd/MM/yyyy}",
                html, ct);
        }
    }

    private static string BuildEmailHtml(string sectorName, List<OverdueEpiItemDto> items)
    {
        var rows = string.Join("", items.Select(i => $"""
            <tr>
              <td style="padding:6px 10px;border-bottom:1px solid #eee">{i.EmployeeName}</td>
              <td style="padding:6px 10px;border-bottom:1px solid #eee">{i.EmployeeRegistration}</td>
              <td style="padding:6px 10px;border-bottom:1px solid #eee">{i.Position}</td>
              <td style="padding:6px 10px;border-bottom:1px solid #eee">{i.EmployeeWorkShift ?? "-"}</td>
              <td style="padding:6px 10px;border-bottom:1px solid #eee">{i.EpiName}</td>
              <td style="padding:6px 10px;border-bottom:1px solid #eee;color:#c0392b"><strong>{i.DaysOverdue} dias</strong></td>
              <td style="padding:6px 10px;border-bottom:1px solid #eee">{i.NextReplacementDate.ToLocalTime():dd/MM/yyyy}</td>
            </tr>
        """));

        return $"""
        <html><body style="font-family:Arial,sans-serif;color:#222">
          <h2 style="color:#c0392b">⚠️ EPIs com prazo vencido — {sectorName}</h2>
          <p>Os seguintes funcionários estão com EPIs vencidos e sem troca registrada:</p>
          <table style="border-collapse:collapse;width:100%;font-size:13px">
            <thead>
              <tr style="background:#f0f0f0">
                <th style="padding:8px 10px;text-align:left">Funcionário</th>
                <th style="padding:8px 10px;text-align:left">Matrícula</th>
                <th style="padding:8px 10px;text-align:left">Função</th>
                <th style="padding:8px 10px;text-align:left">Turno</th>
                <th style="padding:8px 10px;text-align:left">EPI</th>
                <th style="padding:8px 10px;text-align:left">Atraso</th>
                <th style="padding:8px 10px;text-align:left">Venceu em</th>
              </tr>
            </thead>
            <tbody>{rows}</tbody>
          </table>
          <p style="margin-top:20px;font-size:12px;color:#888">Enviado automaticamente pelo sistema Digital-RH.</p>
        </body></html>
        """;
    }
}
