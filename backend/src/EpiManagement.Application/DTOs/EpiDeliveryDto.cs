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
    bool IsFirstDelivery,
    IEnumerable<EpiDeliveryItemDto> Items
);

public record EpiDeliveryItemDto(
    Guid Id,
    Guid EpiId,
    string EpiName,
    string EpiCode,
    int Quantity,
    DateTime NextReplacementDate,
    bool IsEarlyReplacement,
    string? EarlyReplacementReason
);

public record CreateEpiDeliveryDto(
    Guid EmployeeId,
    string? BiometricSignatureBase64,
    string? Notes,
    IEnumerable<CreateEpiDeliveryItemDto> Items
);

public record CreateEpiDeliveryItemDto(
    Guid EpiId,
    int Quantity,
    bool IsEarlyReplacement = false,
    string? EarlyReplacementReason = null
);

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

public record SuggestedEpiDto(
    Guid EpiId,
    string EpiName,
    string EpiCode,
    bool IsRequired,
    int ReplacementPeriodDays,
    int MaxQuantityAllowed,
    int ValidityDays
);
