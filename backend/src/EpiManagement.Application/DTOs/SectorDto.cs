namespace EpiManagement.Application.DTOs;

public record SectorDto(Guid Id, string Name, string? Description, int EmployeeCount);
public record CreateSectorDto(string Name, string? Description);
public record UpdateSectorDto(string Name, string? Description);
