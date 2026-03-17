using Mediator;
using SA.Operations.Application.Dtos.Applications;
using SA.Operations.Application.Dtos.Timeline;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Queries.Applications;

public sealed class GetTimelineQueryHandler(
    IApplicationRepository applicationRepository,
    ITraceEventRepository traceEventRepository) : IQueryHandler<GetTimelineQuery, TimelineDto>
{
    private static readonly string[] TimelineStates = ["Draft", "Pending", "InReview", "Approved"];

    public async ValueTask<TimelineDto> Handle(GetTimelineQuery query, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(query.ApplicationId, ct);
        if (app is null || app.TenantId != query.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        var traceEvents = await traceEventRepository.GetByApplicationIdAsync(app.Id, ct);
        var currentStatus = app.Status.ToString();

        var completedStates = traceEvents
            .Select(e => e.State)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var nodes = new List<TimelineNodeDto>();
        var reachedCurrent = false;

        foreach (var state in TimelineStates)
        {
            var matchingEvent = traceEvents
                .Where(e => string.Equals(e.State, state, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.OccurredAt)
                .FirstOrDefault();

            string nodeType;
            if (string.Equals(state, currentStatus, StringComparison.OrdinalIgnoreCase))
            {
                nodeType = app.Status == ApplicationStatus.Approved ? "final_approved" : "current";
                reachedCurrent = true;
            }
            else if (matchingEvent is not null)
            {
                nodeType = "completed";
            }
            else
            {
                nodeType = reachedCurrent ? "pending" : "pending";
            }

            nodes.Add(new TimelineNodeDto(
                state,
                nodeType,
                matchingEvent?.Action,
                matchingEvent?.UserName,
                matchingEvent is not null ? matchingEvent.OccurredAt : null,
                matchingEvent?.Details
                    .Select(d => new TraceEventDetailDto(d.Key, d.Value))
                    .ToArray()));
        }

        // Handle rejected as final node if current status is Rejected
        if (app.Status == ApplicationStatus.Rejected)
        {
            var rejectedEvent = traceEvents
                .Where(e => string.Equals(e.State, "Rejected", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.OccurredAt)
                .FirstOrDefault();

            nodes.Add(new TimelineNodeDto(
                "Rejected",
                "final_rejected",
                rejectedEvent?.Action,
                rejectedEvent?.UserName,
                rejectedEvent is not null ? rejectedEvent.OccurredAt : null,
                rejectedEvent?.Details
                    .Select(d => new TraceEventDetailDto(d.Key, d.Value))
                    .ToArray()));
        }

        return new TimelineDto(currentStatus, app.WorkflowStage, nodes.ToArray());
    }
}
