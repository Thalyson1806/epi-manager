using EpiManagement.Domain.Enums;

namespace EpiManagement.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<EpiDelivery> Deliveries { get; private set; } = new List<EpiDelivery>();

    protected User() { }

    public User(string name, string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string email, UserRole role)
    {
        Name = name;
        Email = email;
        Role = role;
    }

    public void SetPassword(string passwordHash) => PasswordHash = passwordHash;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
