namespace EpiManagement.Domain.Entities;

public class Sector
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<Employee> Employees { get; private set; } = new List<Employee>();
    public ICollection<SectorEpi> SectorEpis { get; private set; } = new List<SectorEpi>();

    protected Sector() { }

    public Sector(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
