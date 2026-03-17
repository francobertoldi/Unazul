using Mediator;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct LogoutCommand(
    Guid UserId,
    string? RefreshToken) : ICommand;
