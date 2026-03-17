using Mediator;

namespace SA.Catalog.Application.Commands.Products;

public readonly record struct UpdateProductCommand(
    Guid TenantId, Guid Id,
    string Name, string Code, string? Description,
    string Status, string ValidFrom, string? ValidTo,
    Guid UserId) : ICommand;
