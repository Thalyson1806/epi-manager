using EpiManagement.Domain.Enums;

namespace EpiManagement.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    string Name,
    string Cpf,
    string Registration,
    Guid SectorId,
    string SectorName,
    string Position,
    string? WorkShift,
    DateTime AdmissionDate,
    EmployeeStatus Status,
    bool HasBiometric,
    string? PhotoUrl,
    DateTime CreatedAt
);

public record CreateEmployeeDto(
    string Name,
    string Cpf,
    string Registration,
    Guid SectorId,
    string Position,
    string? WorkShift,
    DateTime AdmissionDate
);

public record UpdateEmployeeDto(
    string Name,
    string Cpf,
    string Registration,
    Guid SectorId,
    string Position,
    string? WorkShift,
    DateTime AdmissionDate
);

public record EmployeeBiometricDto(
    Guid EmployeeId,
    string TemplateBase64
);
