namespace EpiManagement.Domain.Entities;

public class EpiDeliveryItem
{
    public Guid Id { get; private set; }
    public Guid EpiDeliveryId { get; private set; }
    public EpiDelivery EpiDelivery { get; private set; } = null!;
    public Guid EpiId { get; private set; }
    public Epi Epi { get; private set; } = null!;
    public int Quantity { get; private set; }
    public DateTime NextReplacementDate { get; private set; }
    public bool IsEarlyReplacement { get; private set; }
    public string? EarlyReplacementReason { get; private set; }

    protected EpiDeliveryItem() { }

    public EpiDeliveryItem(Guid epiDeliveryId, Guid epiId, int quantity, int validityDays,
        bool isEarlyReplacement = false, string? earlyReplacementReason = null)
    {
        Id = Guid.NewGuid();
        EpiDeliveryId = epiDeliveryId;
        EpiId = epiId;
        Quantity = quantity;
        NextReplacementDate = DateTime.UtcNow.AddDays(validityDays);
        IsEarlyReplacement = isEarlyReplacement;
        EarlyReplacementReason = earlyReplacementReason;
    }
}
