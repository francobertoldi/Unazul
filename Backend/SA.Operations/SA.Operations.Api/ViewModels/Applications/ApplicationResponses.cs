using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Applications;

public sealed record ApplicationListResponse(
    Guid Id,
    string Code,
    string ProductName,
    string PlanName,
    ApplicationStatus Status,
    string? ApplicantName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ApplicationDetailResponse(
    Guid Id,
    string Code,
    Guid EntityId,
    Guid ApplicantId,
    Guid ProductId,
    Guid PlanId,
    string ProductName,
    string PlanName,
    ApplicationStatus Status,
    string? WorkflowStage,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy);

public sealed record TimelineEventResponse(
    Guid Id,
    string State,
    string Action,
    string UserName,
    DateTime OccurredAt,
    IReadOnlyList<TimelineDetailResponse> Details);

public sealed record TimelineDetailResponse(
    string Key,
    string Value);
