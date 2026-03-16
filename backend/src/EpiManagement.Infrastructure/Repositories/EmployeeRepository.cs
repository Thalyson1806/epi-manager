using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;
using EpiManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EpiManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _ctx;

    public EmployeeRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Employees.Include(e => e.Sector).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Employee?> GetByCpfAsync(string cpf, CancellationToken ct = default)
        => await _ctx.Employees.Include(e => e.Sector).FirstOrDefaultAsync(e => e.Cpf == cpf, ct);

    public async Task<Employee?> GetByRegistrationAsync(string registration, CancellationToken ct = default)
        => await _ctx.Employees.Include(e => e.Sector).FirstOrDefaultAsync(e => e.Registration == registration, ct);

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Employees.Include(e => e.Sector).ToListAsync(ct);

    public async Task<IEnumerable<Employee>> GetBySectorAsync(Guid sectorId, CancellationToken ct = default)
        => await _ctx.Employees.Include(e => e.Sector).Where(e => e.SectorId == sectorId).ToListAsync(ct);

    public async Task<IEnumerable<Employee>> GetWithBiometricTemplatesAsync(CancellationToken ct = default)
        => await _ctx.Employees.Include(e => e.Sector)
            .Where(e => e.BiometricTemplate != null).ToListAsync(ct);

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
        => await _ctx.Employees.AddAsync(employee, ct);

    public Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        _ctx.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsCpfAsync(string cpf, Guid? excludeId = null, CancellationToken ct = default)
        => await _ctx.Employees.AnyAsync(e => e.Cpf == cpf && (excludeId == null || e.Id != excludeId), ct);

    public async Task<bool> ExistsRegistrationAsync(string registration, Guid? excludeId = null, CancellationToken ct = default)
        => await _ctx.Employees.AnyAsync(e => e.Registration == registration && (excludeId == null || e.Id != excludeId), ct);
}
