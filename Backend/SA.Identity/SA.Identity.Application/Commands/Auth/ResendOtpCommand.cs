using Mediator;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct ResendOtpCommand(
    Guid UserId) : ICommand;
