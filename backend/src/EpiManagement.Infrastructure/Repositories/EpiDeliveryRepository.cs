using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class EpiDeliveryRepository : IEpiDeliveryRepository
{
    private readonly AppDbContext _ctx;

    public EpiDeliveryRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<EpiDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.EpiDeliveries
            .Include(d => d.Employee).ThenInclude(e => e.Sector)
            .Include(d => d.Operator)
            .Include(d => d.Items).ThenInclude(i => i.Epi)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IEnumerable<EpiDelivery>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
        => await _ctx.EpiDeliveries
            .Include(d => d.Employee).ThenInclude(e => e.Sector)
            .Include(d => d.Operator)
            .Include(d => d.Items).ThenInclude(i => i.Epi)
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.DeliveryDate)
            .ToListAsync(ct);

    public async Task<EpiDelivery?> GetLastDeliveryForEpiAsync(Guid employeeId, Guid epiId, CancellationToken ct = default)
        => await _ctx.EpiDeliveries
            .Include(d => d.Items)
            .Where(d => d.EmployeeId == employeeId && d.Items.Any(i => i.EpiId == epiId))
            .OrderByDescending(d => d.DeliveryDate)
            .FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<EpiDelivery>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default)
        => await _ctx.EpiDeliveries
            .Include(d => d.Employee).ThenInclude(e => e.Sector)
            .Include(d => d.Items)
            .Where(d => d.DeliveryDate >= start && d.DeliveryDate <= end)
            .ToListAsync(ct);

    public async Task<int> GetTodayDeliveriesCountAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _ctx.EpiDeliveries.CountAsync(d => d.DeliveryDate.Date == today, ct);
    }

    public async Task<int> GetTodayEmployeesAttendedCountAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _ctx.EpiDeliveries
            .Where(d => d.DeliveryDate.Date == today)
            .Select(d => d.EmployeeId).Distinct().CountAsync(ct);
    }

    public async Task<IEnumerable<EpiDeliveryItem>> GetExpiringItemsAsync(int daysAhead, CancellationToken ct = default)
    {
        var limit = DateTime.UtcNow.AddDays(daysAhead);
        return await _ctx.EpiDeliveryItems
            .Include(i => i.Epi)
            .Include(i => i.EpiDelivery).ThenInclude(d => d.Employee)
            .Where(i => i.NextReplacementDate <= limit && i.NextReplacementDate >= DateTime.UtcNow)
            .ToListAsync(ct);
    }

    public async Task AddAsync(EpiDelivery delivery, CancellationToken ct = default)
        => await _ctx.EpiDeliveries.AddAsync(delivery, ct);
}
