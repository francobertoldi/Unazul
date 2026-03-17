using SA.Operations.Api.ViewModels.Observations;
using SA.Operations.Domain.Entities;

namespace SA.Operations.Api.Mappers.Observations;

public static class ObservationMapper
{
    public static ObservationResponse ToResponse(ApplicationObservation obs)
    {
        return new ObservationResponse(
            obs.Id,
            obs.ObservationType,
            obs.Content,
            obs.UserId,
            obs.UserName,
            obs.CreatedAt);
    }
}
