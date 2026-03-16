namespace EpiManagement.Domain.Entities;

public class EpiDelivery
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid OperatorId { get; private set; }
    public User Operator { get; private set; } = null!;
    public DateTime DeliveryDate { get; private set; }
    public byte[]? BiometricSignature { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<EpiDeliveryItem> Items { get; private set; } = new List<EpiDeliveryItem>();

    protected EpiDelivery() { }

    public EpiDelivery(Guid employeeId, Guid operatorId, byte[]? biometricSignature, string? notes)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        OperatorId = operatorId;
        DeliveryDate = DateTime.UtcNow;
        BiometricSignature = biometricSignature;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(EpiDeliveryItem item)
    {
        Items.Add(item);
    }
}
