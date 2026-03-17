using Mediator;

namespace SA.Operations.Application.Commands.Contacts;

public readonly record struct CreateContactCommand(
    Guid ApplicantId,
    Guid TenantId,
    string Type,
    string? Email,
    string? PhoneCode,
    string? Phone) : ICommand<CreateContactResult>;

public sealed record CreateContactResult(Guid Id, string Type);
