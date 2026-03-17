using Mediator;

namespace SA.Catalog.Application.Commands.Coverages;

public readonly record struct AddCoverageCommand(
    Guid TenantId, Guid PlanId,
    string Name, string CoverageType,
    decimal SumInsured, decimal? Premium, int? GracePeriodDays,
    Guid UserId) : ICommand<Guid>;
