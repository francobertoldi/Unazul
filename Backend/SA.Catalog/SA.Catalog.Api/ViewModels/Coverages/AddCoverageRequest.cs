namespace SA.Catalog.Api.ViewModels.Coverages;

public sealed record AddCoverageRequest(
    string Name,
    string CoverageType,
    decimal SumInsured,
    decimal? Premium,
    int? GracePeriodDays);
