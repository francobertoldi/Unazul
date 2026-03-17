using Mediator;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct ForgotPasswordCommand(
    Guid TenantId,
    string Email) : ICommand;
