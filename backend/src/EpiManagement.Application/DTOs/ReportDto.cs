namespace EpiManagement.Application.DTOs;

public record DashboardDto(
    int TodayDeliveries,
    int TodayEmployeesAttended,
    int ExpiringEpisNext30Days,
    int ActiveEmployees,
    IEnumerable<RecentDeliveryDto> RecentDeliveries
);

public record RecentDeliveryDto(
    Guid DeliveryId,
    string EmployeeName,
    string SectorName,
    DateTime DeliveryDate,
    int ItemsCount
);

public record EpiDeliveryFilterDto(
    Guid? EmployeeId,
    Guid? SectorId,
    Guid? EpiId,
    DateTime? StartDate,
    DateTime? EndDate
);
