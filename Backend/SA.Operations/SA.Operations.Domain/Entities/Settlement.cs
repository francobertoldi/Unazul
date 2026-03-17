namespace SA.Operations.Domain.Entities;

public sealed class Settlement
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTime SettledAt { get; private set; }
    public Guid SettledBy { get; private set; }
    public string SettledByName { get; private set; } = string.Empty;
    public int OperationCount { get; private set; }
    public string? ExcelUrl { get; private set; }

    public ICollection<SettlementTotal> Totals { get; private set; } = [];
    public ICollection<SettlementItem> Items { get; private set; } = [];

    private Settlement() { }

    public static Settlement Create(
        Guid tenantId,
        Guid settledBy,
        string settledByName,
        int operationCount)
    {
        return new Settlement
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            SettledAt = DateTime.UtcNow,
            SettledBy = settledBy,
            SettledByName = settledByName,
            OperationCount = operationCount
        };
    }

    public void SetExcelUrl(string url)
    {
        ExcelUrl = url;
    }
}
