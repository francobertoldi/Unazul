using Mediator;

namespace SA.Operations.Application.Commands.Addresses;

public readonly record struct DeleteAddressCommand(
    Guid AddressId,
    Guid TenantId) : ICommand<DeleteAddressResult>;

public sealed record DeleteAddressResult(Guid Id);
