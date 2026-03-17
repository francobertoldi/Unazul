using Mediator;

namespace SA.Catalog.Application.Commands.Products;

public readonly record struct CreateProductCommand(
    Guid TenantId, Guid EntityId, Guid FamilyId,
    string Name, string Code, string? Description,
    string Status, string ValidFrom, string? ValidTo,
    Guid UserId) : ICommand<Guid>;
