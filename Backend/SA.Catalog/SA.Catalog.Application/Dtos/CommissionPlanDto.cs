namespace SA.Catalog.Application.Dtos;

public sealed record CommissionPlanDto(
    Guid Id, string Code, string Description, string Type,
    decimal Value, decimal? MaxAmount, int AssignedPlanCount, DateTime CreatedAt);
