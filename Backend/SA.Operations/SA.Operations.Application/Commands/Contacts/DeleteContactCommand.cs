using Mediator;

namespace SA.Operations.Application.Commands.Contacts;

public readonly record struct DeleteContactCommand(
    Guid ContactId,
    Guid TenantId) : ICommand<DeleteContactResult>;

public sealed record DeleteContactResult(Guid Id);
