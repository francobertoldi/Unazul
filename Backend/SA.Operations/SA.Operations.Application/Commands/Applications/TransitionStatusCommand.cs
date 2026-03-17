using Mediator;

namespace SA.Operations.Application.Commands.Applications;

public readonly record struct TransitionStatusCommand(
    Guid ApplicationId,
    Guid TenantId,
    string NewStatus,
    string Action,
    string? Detail,
    Guid UserId,
    string UserName) : ICommand<TransitionStatusResult>;

public sealed record TransitionStatusResult(Guid Id, string Code, string Status);
