namespace EpiManagement.Domain.Entities;

public class SystemConfig
{
    public Guid Id { get; private set; } = new Guid("00000000-0000-0000-0000-000000000001");
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SmtpFromEmail { get; set; } = string.Empty;
    public string SmtpFromName { get; set; } = string.Empty;
    public bool SmtpUseSsl { get; set; } = false;
    public bool AlertEnabled { get; set; } = false;
    public int AlertHour { get; set; } = 6;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
