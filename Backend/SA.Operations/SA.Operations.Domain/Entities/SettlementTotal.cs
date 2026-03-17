namespace SA.Operations.Domain.Entities;

public sealed class SettlementTotal
{
    public Guid Id { get; private set; }
    public Guid SettlementId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public int ItemCount { get; private set; }

    private SettlementTotal() { }

    public static SettlementTotal Create(
        Guid settlementId,
        Guid tenantId,
        string currency,
        decimal totalAmount,
        int itemCount)
    {
        return new SettlementTotal
        {
            Id = Guid.CreateVersion7(),
            SettlementId = settlementId,
            TenantId = tenantId,
            Currency = currency,
            TotalAmount = totalAmount,
            ItemCount = itemCount
        };
    }
}
