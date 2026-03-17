namespace SA.Catalog.Api.ViewModels.CommissionPlans;

public sealed record CreateCommissionPlanRequest(
    string Code,
    string Description,
    string Type,
    decimal Value,
    decimal? MaxAmount);
