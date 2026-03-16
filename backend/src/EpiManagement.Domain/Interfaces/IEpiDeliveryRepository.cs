using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface IEpiDeliveryRepository
{
    Task<EpiDelivery?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<EpiDelivery>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<EpiDelivery?> GetLastDeliveryForEpiAsync(Guid employeeId, Guid epiId, CancellationToken ct = default);
    Task<IEnumerable<EpiDelivery>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task<int> GetTodayDeliveriesCountAsync(CancellationToken ct = default);
    Task<int> GetTodayEmployeesAttendedCountAsync(CancellationToken ct = default);
    Task<IEnumerable<EpiDeliveryItem>> GetExpiringItemsAsync(int daysAhead, CancellationToken ct = default);
    Task AddAsync(EpiDelivery delivery, CancellationToken ct = default);
}
