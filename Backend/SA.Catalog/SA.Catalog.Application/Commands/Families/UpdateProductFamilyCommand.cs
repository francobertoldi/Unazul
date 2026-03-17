using Mediator;

namespace SA.Catalog.Application.Commands.Families;

public readonly record struct UpdateProductFamilyCommand(
    Guid TenantId, Guid Id, string Description, Guid UserId) : ICommand;
