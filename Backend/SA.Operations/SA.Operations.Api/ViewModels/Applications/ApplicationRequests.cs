using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Applications;

public sealed record CreateApplicationRequest(
    Guid EntityId,
    Guid ApplicantId,
    Guid ProductId,
    Guid PlanId);

public sealed record UpdateApplicationDraftRequest(
    Guid? EntityId,
    Guid? ProductId,
    Guid? PlanId);

public sealed record TransitionStatusRequest(
    ApplicationStatus NewStatus,
    string? Comment);
