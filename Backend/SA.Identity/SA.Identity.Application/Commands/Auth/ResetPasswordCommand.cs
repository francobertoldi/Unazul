using Mediator;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct ResetPasswordCommand(
    string Token,
    string NewPassword) : ICommand;
