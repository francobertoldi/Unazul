using Mediator;

namespace SA.Catalog.Application.Commands.Families;

public readonly record struct DeleteProductFamilyCommand(Guid TenantId, Guid Id) : ICommand;
