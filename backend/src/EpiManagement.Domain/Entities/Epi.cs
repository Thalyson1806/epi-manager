namespace EpiManagement.Domain.Entities;

public class Epi
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int ValidityDays { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<SectorEpi> SectorEpis { get; private set; } = new List<SectorEpi>();
    public ICollection<EpiDeliveryItem> DeliveryItems { get; private set; } = new List<EpiDeliveryItem>();

    protected Epi() { }

    public Epi(string name, string code, string? description, int validityDays, string type)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        Description = description;
        ValidityDays = validityDays;
        Type = type;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string code, string? description, int validityDays, string type)
    {
        Name = name;
        Code = code;
        Description = description;
        ValidityDays = validityDays;
        Type = type;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
