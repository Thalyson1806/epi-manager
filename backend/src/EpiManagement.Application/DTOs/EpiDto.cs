namespace EpiManagement.Application.DTOs;

public record EpiDto(Guid Id, string Name, string Code, string? Description, int ValidityDays, string Type, bool IsActive);
public record CreateEpiDto(string Name, string Code, string? Description, int ValidityDays, string Type);
public record UpdateEpiDto(string Name, string Code, string? Description, int ValidityDays, string Type);
