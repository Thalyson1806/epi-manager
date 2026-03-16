namespace EpiManagement.Domain.Interfaces;

public interface IUnitOfWork
{
    IEmployeeRepository Employees { get; }
    ISectorRepository Sectors { get; }
    IEpiRepository Epis { get; }
    IEpiDeliveryRepository EpiDeliveries { get; }
    IUserRepository Users { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
