using Mediator;

namespace SA.Operations.Application.Commands.Contacts;

public readonly record struct UpdateContactCommand(
    Guid ContactId,
    Guid TenantId,
    string Type,
    string? Email,
    string? PhoneCode,
    string? Phone) : ICommand<UpdateContactResult>;

public sealed record UpdateContactResult(Guid Id, string Type);
