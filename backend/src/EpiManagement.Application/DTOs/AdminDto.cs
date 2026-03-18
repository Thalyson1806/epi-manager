namespace EpiManagement.Application.DTOs;

public record SmtpConfigDto(
    string SmtpHost,
    int SmtpPort,
    string SmtpUser,
    string SmtpPassword,
    string SmtpFromEmail,
    string SmtpFromName,
    bool SmtpUseSsl,
    bool AlertEnabled,
    int AlertHour
);

public record OverdueEpiItemDto(
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeRegistration,
    string? EmployeeWorkShift,
    string Position,
    string SectorName,
    string? SectorSupervisorEmail,
    Guid EpiId,
    string EpiName,
    string EpiCode,
    DateTime NextReplacementDate,
    int DaysOverdue
);
