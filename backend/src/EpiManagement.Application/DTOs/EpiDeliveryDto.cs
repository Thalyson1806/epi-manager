namespace EpiManagement.Application.DTOs;

public record EpiDeliveryDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeRegistration,
    string SectorName,
    Guid OperatorId,
    string OperatorName,
    DateTime DeliveryDate,
    bool HasBiometricSignature,
    string? Notes,
    IEnumerable<EpiDeliveryItemDto> Items
);

public record EpiDeliveryItemDto(
    Guid Id,
    Guid EpiId,
    string EpiName,
    string EpiCode,
    int Quantity,
    DateTime NextReplacementDate
);

public record CreateEpiDeliveryDto(
    Guid EmployeeId,
    string? BiometricSignatureBase64,
    string? Notes,
    IEnumerable<CreateEpiDeliveryItemDto> Items
);

public record CreateEpiDeliveryItemDto(Guid EpiId, int Quantity);

public record BiometricIdentifyDto(string BiometricSampleBase64);

public record BiometricIdentifyResultDto(
    bool Identified,
    Guid? EmployeeId,
    string? EmployeeName,
    string? Registration,
    string? SectorName,
    string? Position,
    string? PhotoUrl
);
