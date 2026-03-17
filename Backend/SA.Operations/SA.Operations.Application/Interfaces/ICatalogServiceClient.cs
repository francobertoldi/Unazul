namespace SA.Operations.Application.Interfaces;

public interface ICatalogServiceClient
{
    Task<CatalogProductResult?> ValidateProductAndPlanAsync(Guid productId, Guid planId, CancellationToken ct = default);
    Task<IReadOnlyList<CommissionPlanResult>> GetCommissionPlansAsync(Guid[] productIds, Guid[] planIds, CancellationToken ct = default);
}

public sealed record CatalogProductResult(Guid ProductId, string ProductName, Guid PlanId, string PlanName, bool IsActive);
public sealed record CommissionPlanResult(Guid ProductId, Guid PlanId, string? CommissionType, decimal? CommissionValue, string Currency, string? FormulaDescription);
