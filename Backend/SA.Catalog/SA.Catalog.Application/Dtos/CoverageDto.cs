namespace SA.Catalog.Application.Dtos;

public sealed record CoverageDto(
    Guid Id, string Name, string CoverageType,
    decimal SumInsured, decimal? Premium, int? GracePeriodDays);
