using Mediator;

namespace SA.Catalog.Application.Commands.Requirements;

public readonly record struct CreateRequirementCommand(
    Guid TenantId, Guid ProductId,
    string Name, string Type, bool IsMandatory, string? Description,
    Guid UserId) : ICommand<Guid>;
