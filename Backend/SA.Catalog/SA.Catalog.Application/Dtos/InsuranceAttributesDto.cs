namespace SA.Catalog.Application.Dtos;

public sealed record InsuranceAttributesDto(
    decimal Premium, decimal SumInsured,
    int? GracePeriodDays, string CoverageType);
