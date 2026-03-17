using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Observations;

public sealed record CreateObservationRequest(
    ObservationType ObservationType,
    string Content);
