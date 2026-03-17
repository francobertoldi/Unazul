using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Observations;

public sealed record ObservationResponse(
    Guid Id,
    ObservationType ObservationType,
    string Content,
    Guid UserId,
    string UserName,
    DateTime CreatedAt);
