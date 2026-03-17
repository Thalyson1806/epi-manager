using EpiManagement.Domain.Enums;

namespace EpiManagement.Domain.Entities;

public class Employee
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Registration { get; private set; } = string.Empty;
    public Guid SectorId { get; private set; }
    public Sector Sector { get; private set; } = null!;
    public string Position { get; private set; } = string.Empty;
    public string? WorkShift { get; private set; }
    public DateTime AdmissionDate { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public byte[]? BiometricTemplate { get; private set; }
    public string? PhotoUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public ICollection<EpiDelivery> EpiDeliveries { get; private set; } = new List<EpiDelivery>();

    protected Employee() { }

    public Employee(string name, string cpf, string registration, Guid sectorId, string position, DateTime admissionDate, string? workShift = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Cpf = cpf;
        Registration = registration;
        SectorId = sectorId;
        Position = position;
        WorkShift = workShift;
        AdmissionDate = admissionDate;
        Status = EmployeeStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string cpf, string registration, Guid sectorId, string position, DateTime admissionDate, string? workShift = null)
    {
        Name = name;
        Cpf = cpf;
        Registration = registration;
        SectorId = sectorId;
        Position = position;
        WorkShift = workShift;
        AdmissionDate = admissionDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBiometricTemplate(byte[]? template)
    {
        BiometricTemplate = template;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPhoto(string photoUrl)
    {
        PhotoUrl = photoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = EmployeeStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = EmployeeStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
}
