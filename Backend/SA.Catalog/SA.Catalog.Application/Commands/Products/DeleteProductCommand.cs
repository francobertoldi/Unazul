using Mediator;

namespace SA.Catalog.Application.Commands.Products;

public readonly record struct DeleteProductCommand(Guid TenantId, Guid Id) : ICommand;
