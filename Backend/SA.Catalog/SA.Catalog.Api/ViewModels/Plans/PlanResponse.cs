using SA.Catalog.Api.ViewModels.Coverages;

namespace SA.Catalog.Api.ViewModels.Plans;

public sealed record PlanResponse(
    Guid Id,
    string Name,
    string Code,
    decimal Price,
    string Currency,
    int? Installments,
    Guid? CommissionPlanId,
    string? CommissionPlanCode,
    string? CommissionPlanDescription,
    object? CategoryAttributes,
    IReadOnlyList<CoverageResponse>? Coverages);
