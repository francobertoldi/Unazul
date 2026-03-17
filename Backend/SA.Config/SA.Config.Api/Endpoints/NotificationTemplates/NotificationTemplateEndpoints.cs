using Mediator;
using SA.Config.Api.Mappers.NotificationTemplates;
using SA.Config.Api.ViewModels.NotificationTemplates;
using SA.Config.Application.Commands.NotificationTemplates;
using SA.Config.Application.Queries.NotificationTemplates;
using Shared.Auth;

namespace SA.Config.Api.Endpoints.NotificationTemplates;

public static class NotificationTemplateEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/notification-templates")
            .WithTags("NotificationTemplates")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-CFG-19: List notification templates
        group.MapGet("/", async (
            int page,
            int page_size,
            string? channel,
            string? search,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new ListNotificationTemplatesQuery(
                page,
                page_size,
                channel,
                search));

            return Results.Ok(NotificationTemplateMapper.ToListResponse(
                result.Items, result.Total, result.Page, result.PageSize));
        })
        .Produces<NotificationTemplateListResponse>(200);

        // RF-CFG-19: Create notification template
        group.MapPost("/", async (
            CreateNotificationTemplateRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new CreateNotificationTemplateCommand(
                currentUser.TenantId,
                request.Code,
                request.Name,
                request.Channel,
                request.Subject,
                request.Body,
                request.Status,
                currentUser.UserId));

            return Results.Created($"/api/v1/notification-templates/{result.Id}",
                NotificationTemplateMapper.ToDetailResponse(result));
        })
        .Produces<NotificationTemplateDetailResponse>(201);

        // RF-CFG-19: Update notification template
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateNotificationTemplateRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            var result = await mediator.Send(new UpdateNotificationTemplateCommand(
                id,
                request.Name,
                request.Subject,
                request.Body,
                request.Status,
                currentUser.UserId));

            return Results.Ok(NotificationTemplateMapper.ToDetailResponse(result));
        })
        .Produces<NotificationTemplateDetailResponse>(200);

        // RF-CFG-19: Delete notification template
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            await mediator.Send(new DeleteNotificationTemplateCommand(id));
            return Results.NoContent();
        })
        .Produces(204);
    }
}
