using EpiManagement.Domain.Entities;

namespace EpiManagement.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetByCpfAsync(string cpf, CancellationToken ct = default);
    Task<Employee?> GetByRegistrationAsync(string registration, CancellationToken ct = default);
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Employee>> GetBySectorAsync(Guid sectorId, CancellationToken ct = default);
    Task<IEnumerable<Employee>> GetWithBiometricTemplatesAsync(CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task<bool> ExistsCpfAsync(string cpf, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> ExistsRegistrationAsync(string registration, Guid? excludeId = null, CancellationToken ct = default);
}
