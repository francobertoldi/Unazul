namespace SA.Catalog.Api.ViewModels.Coverages;

public sealed record UpdateCoverageRequest(
    decimal SumInsured,
    decimal? Premium,
    int? GracePeriodDays);
