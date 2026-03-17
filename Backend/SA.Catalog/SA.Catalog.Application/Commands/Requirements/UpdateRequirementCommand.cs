using Mediator;

namespace SA.Catalog.Application.Commands.Requirements;

public readonly record struct UpdateRequirementCommand(
    Guid TenantId, Guid Id, Guid ProductId,
    string Name, string Type, bool IsMandatory, string? Description,
    Guid UserId) : ICommand;
