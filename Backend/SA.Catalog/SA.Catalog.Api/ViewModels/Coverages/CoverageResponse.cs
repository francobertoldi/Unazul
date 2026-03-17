namespace SA.Catalog.Api.ViewModels.Coverages;

public sealed record CoverageResponse(
    Guid Id,
    string Name,
    string CoverageType,
    decimal SumInsured,
    decimal? Premium,
    int? GracePeriodDays);
