using Mediator;
using SA.Identity.Application.Dtos.Auth;

namespace SA.Identity.Application.Commands.Auth;

public readonly record struct VerifyOtpCommand(
    Guid TenantId,
    Guid UserId,
    string OtpCode) : ICommand<TokenDto>;
