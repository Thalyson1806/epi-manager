using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Repositories;

namespace EpiManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _ctx;

    public UnitOfWork(
        AppDbContext ctx,
        IEmployeeRepository employees,
        ISectorRepository sectors,
        IEpiRepository epis,
        IEpiDeliveryRepository epiDeliveries,
        IUserRepository users,
        IAuditLogRepository auditLogs)
    {
        _ctx = ctx;
        Employees = employees;
        Sectors = sectors;
        Epis = epis;
        EpiDeliveries = epiDeliveries;
        Users = users;
        AuditLogs = auditLogs;
    }

    public IEmployeeRepository Employees { get; }
    public ISectorRepository Sectors { get; }
    public IEpiRepository Epis { get; }
    public IEpiDeliveryRepository EpiDeliveries { get; }
    public IUserRepository Users { get; }
    public IAuditLogRepository AuditLogs { get; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _ctx.SaveChangesAsync(ct);
}
