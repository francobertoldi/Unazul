using Mediator;

namespace SA.Catalog.Application.Commands.Families;

public readonly record struct CreateProductFamilyCommand(
    Guid TenantId, string Code, string Description, Guid UserId) : ICommand<Guid>;
