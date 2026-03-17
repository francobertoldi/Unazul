using Mediator;
using SA.Operations.Application.Dtos.Timeline;

namespace SA.Operations.Application.Queries.Applications;

public readonly record struct GetTimelineQuery(
    Guid ApplicationId,
    Guid TenantId) : IQuery<TimelineDto>;
