namespace EpiManagement.Application.DTOs;

public record SectorDto(Guid Id, string Name, string? Description, int EmployeeCount, string? SupervisorName, string? SupervisorEmail);
public record CreateSectorDto(string Name, string? Description, string? SupervisorName, string? SupervisorEmail);
public record UpdateSectorDto(string Name, string? Description, string? SupervisorName, string? SupervisorEmail);
