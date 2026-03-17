namespace SA.Operations.Domain.Entities;

public sealed class Beneficiary
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Relationship { get; private set; } = string.Empty;
    public decimal Percentage { get; private set; }

    private Beneficiary() { }

    public static Beneficiary Create(
        Guid applicationId,
        Guid tenantId,
        string firstName,
        string lastName,
        string relationship,
        decimal percentage)
    {
        return new Beneficiary
        {
            Id = Guid.CreateVersion7(),
            ApplicationId = applicationId,
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            Relationship = relationship,
            Percentage = percentage
        };
    }

    public void Update(
        string firstName,
        string lastName,
        string relationship,
        decimal percentage)
    {
        FirstName = firstName;
        LastName = lastName;
        Relationship = relationship;
        Percentage = percentage;
    }
}
