using SA.Operations.Api.ViewModels.Applications;
using SA.Operations.Domain.Entities;
using DomainApplication = SA.Operations.Domain.Entities.Application;

namespace SA.Operations.Api.Mappers.Applications;

public static class ApplicationMapper
{
    public static ApplicationListResponse ToListResponse(DomainApplication app, string? applicantName = null)
    {
        return new ApplicationListResponse(
            app.Id,
            app.Code,
            app.ProductName,
            app.PlanName,
            app.Status,
            applicantName,
            app.CreatedAt,
            app.UpdatedAt);
    }

    public static ApplicationDetailResponse ToDetailResponse(DomainApplication app)
    {
        return new ApplicationDetailResponse(
            app.Id,
            app.Code,
            app.EntityId,
            app.ApplicantId,
            app.ProductId,
            app.PlanId,
            app.ProductName,
            app.PlanName,
            app.Status,
            app.WorkflowStage,
            app.CreatedAt,
            app.UpdatedAt,
            app.CreatedBy,
            app.UpdatedBy);
    }

    public static TimelineEventResponse ToTimelineResponse(TraceEvent evt)
    {
        return new TimelineEventResponse(
            evt.Id,
            evt.State,
            evt.Action,
            evt.UserName,
            evt.OccurredAt,
            evt.Details.Select(d => new TimelineDetailResponse(d.Key, d.Value)).ToList());
    }
}
