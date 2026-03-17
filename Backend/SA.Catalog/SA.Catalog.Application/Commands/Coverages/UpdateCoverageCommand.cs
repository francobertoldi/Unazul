using Mediator;

namespace SA.Catalog.Application.Commands.Coverages;

public readonly record struct UpdateCoverageCommand(
    Guid TenantId, Guid Id,
    decimal SumInsured, decimal? Premium, int? GracePeriodDays) : ICommand;
