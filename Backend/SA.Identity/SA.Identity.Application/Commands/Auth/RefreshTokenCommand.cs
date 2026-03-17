using Mediator;
using SA.Identity.Application.Dtos.Auth;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct RefreshTokenCommand(
    string RefreshToken) : ICommand<TokenDto>;
