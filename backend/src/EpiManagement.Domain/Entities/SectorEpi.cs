namespace EpiManagement.Domain.Entities;

public class SectorEpi
{
    public Guid Id { get; private set; }
    public Guid SectorId { get; private set; }
    public Sector Sector { get; private set; } = null!;
    public Guid EpiId { get; private set; }
    public Epi Epi { get; private set; } = null!;
    public bool IsRequired { get; private set; }
    public int ReplacementPeriodDays { get; private set; }
    public int MaxQuantityAllowed { get; private set; }

    protected SectorEpi() { }

    public SectorEpi(Guid sectorId, Guid epiId, bool isRequired, int replacementPeriodDays, int maxQuantityAllowed)
    {
        Id = Guid.NewGuid();
        SectorId = sectorId;
        EpiId = epiId;
        IsRequired = isRequired;
        ReplacementPeriodDays = replacementPeriodDays;
        MaxQuantityAllowed = maxQuantityAllowed;
    }

    public void Update(bool isRequired, int replacementPeriodDays, int maxQuantityAllowed)
    {
        IsRequired = isRequired;
        ReplacementPeriodDays = replacementPeriodDays;
        MaxQuantityAllowed = maxQuantityAllowed;
    }
}
