using Mediator;
using SA.Identity.Application.Dtos.Auth;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct LoginCommand(
    Guid TenantId,
    string Username,
    string Password) : ICommand<LoginDto>;
