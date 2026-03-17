namespace SA.Catalog.Api.ViewModels.CommissionPlans;

public sealed record CommissionPlanResponse(
    Guid Id,
    string Code,
    string Description,
    string Type,
    decimal Value,
    decimal? MaxAmount,
    int AssignedPlanCount,
    DateTime CreatedAt);
