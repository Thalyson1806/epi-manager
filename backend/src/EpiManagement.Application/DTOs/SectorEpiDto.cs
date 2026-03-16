namespace EpiManagement.Application.DTOs;

public record SectorEpiDto(
    Guid Id,
    Guid SectorId,
    string SectorName,
    Guid EpiId,
    string EpiName,
    bool IsRequired,
    int ReplacementPeriodDays,
    int MaxQuantityAllowed
);

public record CreateSectorEpiDto(
    Guid SectorId,
    Guid EpiId,
    bool IsRequired,
    int ReplacementPeriodDays,
    int MaxQuantityAllowed
);

public record UpdateSectorEpiDto(
    bool IsRequired,
    int ReplacementPeriodDays,
    int MaxQuantityAllowed
);
