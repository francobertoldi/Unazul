using Mediator;

namespace SA.Operations.Application.Commands.Observations;

public readonly record struct CreateObservationCommand(
    Guid ApplicationId,
    Guid TenantId,
    string Content,
    Guid UserId,
    string UserName) : ICommand<CreateObservationResult>;

public sealed record CreateObservationResult(Guid Id, string Content, DateTime CreatedAt);
