using EpiManagement.Application.DTOs;
using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;

namespace EpiManagement.Application.Services;

public class EpiDeliveryService
{
    private readonly IUnitOfWork _uow;
    private readonly IBiometricService _biometric;

    public EpiDeliveryService(IUnitOfWork uow, IBiometricService biometric)
    {
        _uow = uow;
        _biometric = biometric;
    }

    public async Task<BiometricIdentifyResultDto> IdentifyByBiometricAsync(string sampleBase64, CancellationToken ct = default)
    {
        var sample = Convert.FromBase64String(sampleBase64);
        var (matched, employeeId) = await _biometric.IdentifyAsync(sample, ct);

        if (!matched || employeeId is null)
            return new BiometricIdentifyResultDto(false, null, null, null, null, null, null);

        var emp = await _uow.Employees.GetByIdAsync(employeeId.Value, ct);
        if (emp is null)
            return new BiometricIdentifyResultDto(false, null, null, null, null, null, null);

        return new BiometricIdentifyResultDto(
            true,
            emp.Id,
            emp.Name,
            emp.Registration,
            emp.Sector?.Name,
            emp.Position,
            emp.PhotoUrl
        );
    }

    public async Task<EpiDeliveryDto> CreateDeliveryAsync(CreateEpiDeliveryDto dto, Guid operatorId, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(dto.EmployeeId, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        byte[]? biometricSignature = null;
        if (!string.IsNullOrEmpty(dto.BiometricSignatureBase64))
            biometricSignature = Convert.FromBase64String(dto.BiometricSignatureBase64);

        var delivery = new EpiDelivery(dto.EmployeeId, operatorId, biometricSignature, dto.Notes);

        foreach (var item in dto.Items)
        {
            var epi = await _uow.Epis.GetByIdAsync(item.EpiId, ct)
                ?? throw new KeyNotFoundException($"EPI {item.EpiId} não encontrado.");
            delivery.AddItem(new EpiDeliveryItem(delivery.Id, item.EpiId, item.Quantity, epi.ValidityDays));
        }

        await _uow.EpiDeliveries.AddAsync(delivery, ct);
        await _uow.SaveChangesAsync(ct);

        var saved = await _uow.EpiDeliveries.GetByIdAsync(delivery.Id, ct);
        return MapDelivery(saved!);
    }

    public async Task<IEnumerable<EpiDeliveryDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var deliveries = await _uow.EpiDeliveries.GetByEmployeeAsync(employeeId, ct);
        return deliveries.Select(MapDelivery);
    }

    public async Task<EpiDeliveryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _uow.EpiDeliveries.GetByIdAsync(id, ct);
        return d is null ? null : MapDelivery(d);
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var todayDeliveries = await _uow.EpiDeliveries.GetTodayDeliveriesCountAsync(ct);
        var todayEmployees = await _uow.EpiDeliveries.GetTodayEmployeesAttendedCountAsync(ct);
        var expiring = await _uow.EpiDeliveries.GetExpiringItemsAsync(30, ct);
        var employees = await _uow.Employees.GetAllAsync(ct);
        var activeEmployees = employees.Count(e => e.Status == Domain.Enums.EmployeeStatus.Active);

        var recentDeliveries = (await _uow.EpiDeliveries.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, ct))
            .OrderByDescending(d => d.DeliveryDate)
            .Take(10)
            .Select(d => new RecentDeliveryDto(
                d.Id,
                d.Employee?.Name ?? string.Empty,
                d.Employee?.Sector?.Name ?? string.Empty,
                d.DeliveryDate,
                d.Items.Count
            ));

        return new DashboardDto(
            todayDeliveries,
            todayEmployees,
            expiring.Count(),
            activeEmployees,
            recentDeliveries
        );
    }

    private static EpiDeliveryDto MapDelivery(EpiDelivery d) => new(
        d.Id,
        d.EmployeeId,
        d.Employee?.Name ?? string.Empty,
        d.Employee?.Registration ?? string.Empty,
        d.Employee?.Sector?.Name ?? string.Empty,
        d.OperatorId,
        d.Operator?.Name ?? string.Empty,
        d.DeliveryDate,
        d.BiometricSignature != null,
        d.Notes,
        d.Items.Select(i => new EpiDeliveryItemDto(
            i.Id, i.EpiId,
            i.Epi?.Name ?? string.Empty,
            i.Epi?.Code ?? string.Empty,
            i.Quantity,
            i.NextReplacementDate
        ))
    );
}
