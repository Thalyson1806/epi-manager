using EpiManagement.Application.DTOs;
using EpiManagement.Domain.Entities;
using EpiManagement.Domain.Interfaces;

namespace EpiManagement.Application.Services;

public class EmployeeService
{
    private readonly IUnitOfWork _uow;

    public EmployeeService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var employees = await _uow.Employees.GetAllAsync(ct);
        return employees.Select(Map);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct);
        return emp is null ? null : Map(emp);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken ct = default)
    {
        if (await _uow.Employees.ExistsCpfAsync(dto.Cpf, null, ct))
            throw new InvalidOperationException("CPF já cadastrado.");

        if (await _uow.Employees.ExistsRegistrationAsync(dto.Registration, null, ct))
            throw new InvalidOperationException("Matrícula já cadastrada.");

        var sector = await _uow.Sectors.GetByIdAsync(dto.SectorId, ct)
            ?? throw new KeyNotFoundException("Setor não encontrado.");

        var employee = new Employee(dto.Name, dto.Cpf, dto.Registration, dto.SectorId, dto.Position, dto.AdmissionDate, dto.WorkShift);
        await _uow.Employees.AddAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);
        employee = (await _uow.Employees.GetByIdAsync(employee.Id, ct))!;
        return Map(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        if (await _uow.Employees.ExistsCpfAsync(dto.Cpf, id, ct))
            throw new InvalidOperationException("CPF já cadastrado.");

        if (await _uow.Employees.ExistsRegistrationAsync(dto.Registration, id, ct))
            throw new InvalidOperationException("Matrícula já cadastrada.");

        emp.Update(dto.Name, dto.Cpf, dto.Registration, dto.SectorId, dto.Position, dto.AdmissionDate, dto.WorkShift);
        await _uow.Employees.UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
        emp = (await _uow.Employees.GetByIdAsync(id, ct))!;
        return Map(emp);
    }

    public async Task SetBiometricAsync(Guid id, string templateBase64, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");
        emp.SetBiometricTemplate(Convert.FromBase64String(templateBase64));
        await _uow.Employees.UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");
        emp.Activate();
        await _uow.Employees.UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");
        await _uow.Employees.DeleteAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ClearBiometricAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");
        emp.SetBiometricTemplate(null);
        await _uow.Employees.UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var emp = await _uow.Employees.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");
        emp.Deactivate();
        await _uow.Employees.UpdateAsync(emp, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<object>> GetBiometricTemplatesAsync(CancellationToken ct = default)
    {
        var employees = await _uow.Employees.GetAllAsync(ct);
        return employees
            .Where(e => e.BiometricTemplate != null)
            .Select(e => (object)new
            {
                employeeId = e.Id,
                templateBase64 = Convert.ToBase64String(e.BiometricTemplate!)
            });
    }

    private static EmployeeDto Map(Employee e) => new(
        e.Id, e.Name, e.Cpf, e.Registration,
        e.SectorId, e.Sector?.Name ?? string.Empty,
        e.Position, e.WorkShift, e.AdmissionDate, e.Status,
        e.BiometricTemplate != null, e.PhotoUrl, e.CreatedAt
    );
}
