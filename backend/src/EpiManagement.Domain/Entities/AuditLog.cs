namespace EpiManagement.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityName { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected AuditLog() { }

    public AuditLog(Guid? userId, string action, string entityName, Guid? entityId, string? oldValues, string? newValues, string? ipAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        CreatedAt = DateTime.UtcNow;
    }
}
