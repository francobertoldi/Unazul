namespace SA.Catalog.Application.Dtos;

public sealed record ProductPlanDto(
    Guid Id, string Name, string Code, decimal Price, string Currency,
    int? Installments, Guid? CommissionPlanId,
    string? CommissionPlanCode, string? CommissionPlanDescription,
    object? CategoryAttributes, IReadOnlyList<CoverageDto>? Coverages);
