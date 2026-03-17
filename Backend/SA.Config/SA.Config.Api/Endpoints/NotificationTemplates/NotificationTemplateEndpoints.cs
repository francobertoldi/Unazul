using Mediator;
using SA.Config.Api.Mappers.NotificationTemplates;
using SA.Config.Api.ViewModels.NotificationTemplates;
using SA.Config.Application.Commands.NotificationTemplates;
using SA.Config.Application.Queries.NotificationTemplates;
using Shared.Auth;
using Shared.Contract.Models;

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
            try
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
            }
            catch (InvalidOperationException ex) when (ex.Message == "NTPL_DUPLICATE_CODE")
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "NTPL_DUPLICATE_CODE"),
                    statusCode: 409);
            }
            catch (InvalidOperationException ex) when (
                ex.Message is "NTPL_INVALID_CHANNEL" or "NTPL_SUBJECT_REQUIRED_FOR_EMAIL")
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, ex.Message),
                    statusCode: 422);
            }
        })
        .Produces<NotificationTemplateDetailResponse>(201)
        .Produces<ErrorResponse>(409)
        .Produces<ErrorResponse>(422);

        // RF-CFG-19: Update notification template
        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateNotificationTemplateRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            try
            {
                var result = await mediator.Send(new UpdateNotificationTemplateCommand(
                    id,
                    request.Name,
                    request.Subject,
                    request.Body,
                    request.Status,
                    currentUser.UserId));

                return Results.Ok(NotificationTemplateMapper.ToDetailResponse(result));
            }
            catch (InvalidOperationException ex) when (ex.Message == "NTPL_NOT_FOUND")
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex) when (
                ex.Message == "NTPL_SUBJECT_REQUIRED_FOR_EMAIL")
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, ex.Message),
                    statusCode: 422);
            }
        })
        .Produces<NotificationTemplateDetailResponse>(200)
        .Produces(404)
        .Produces<ErrorResponse>(422);

        // RF-CFG-19: Delete notification template
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator) =>
        {
            try
            {
                await mediator.Send(new DeleteNotificationTemplateCommand(id));
                return Results.NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message == "NTPL_NOT_FOUND")
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex) when (ex.Message == "NTPL_REFERENCED_BY_WORKFLOW")
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "NTPL_REFERENCED_BY_WORKFLOW"),
                    statusCode: 409);
            }
        })
        .Produces(204)
        .Produces(404)
        .Produces<ErrorResponse>(409);
    }
}
