namespace SA.Operations.Domain.Entities;

public sealed class SettlementItem
{
    public Guid Id { get; private set; }
    public Guid SettlementId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string AppCode { get; private set; } = string.Empty;
    public string ApplicantName { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string PlanName { get; private set; } = string.Empty;
    public string? CommissionType { get; private set; }
    public decimal? CommissionValue { get; private set; }
    public decimal CalculatedAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? FormulaDescription { get; private set; }

    private SettlementItem() { }

    public static SettlementItem Create(
        Guid settlementId,
        Guid tenantId,
        Guid applicationId,
        string appCode,
        string applicantName,
        string productName,
        string planName,
        string? commissionType,
        decimal? commissionValue,
        decimal calculatedAmount,
        string currency,
        string? formulaDescription)
    {
        return new SettlementItem
        {
            Id = Guid.CreateVersion7(),
            SettlementId = settlementId,
            TenantId = tenantId,
            ApplicationId = applicationId,
            AppCode = appCode,
            ApplicantName = applicantName,
            ProductName = productName,
            PlanName = planName,
            CommissionType = commissionType,
            CommissionValue = commissionValue,
            CalculatedAmount = calculatedAmount,
            Currency = currency,
            FormulaDescription = formulaDescription
        };
    }
}
