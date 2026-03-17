using Mediator;

namespace SA.Catalog.Application.Commands.Products;

public readonly record struct DeprecateProductCommand(Guid TenantId, Guid Id, Guid UserId) : ICommand;
