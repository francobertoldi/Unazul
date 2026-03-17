using SA.Operations.Api.ViewModels.Messages;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using Shared.Auth;
using Shared.Contract.Events;
using Shared.Contract.Models;

namespace SA.Operations.Api.Endpoints.Messages;

public static class MessageEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/applications/{applicationId:guid}/messages")
            .WithTags("Messages")
            .WithOpenApi()
            .RequireAuthorization();

        // RF-OPS-15: Send message
        group.MapPost("/", async (
            Guid applicationId,
            SendMessageRequest request,
            IApplicationRepository applicationRepository,
            IConfigServiceClient configClient,
            IIntegrationEventPublisher publisher,
            ICurrentUser currentUser) =>
        {
            var application = await applicationRepository.GetByIdAsync(applicationId);
            if (application is null)
                return Results.Json(
                    new ErrorResponse("Solicitud no encontrada.", "OPS_APPLICATION_NOT_FOUND"),
                    statusCode: 404);

            var template = await configClient.GetNotificationTemplateAsync(request.TemplateId);
            if (template is null)
                return Results.Json(
                    new ErrorResponse("Plantilla de notificacion no encontrada.", "OPS_TEMPLATE_NOT_FOUND"),
                    statusCode: 404);

            // Publish message event to EventBus for async processing
            await publisher.PublishAsync(new MessageSentEvent(
                currentUser.TenantId,
                applicationId,
                request.Channel.ToString(),
                request.Recipient,
                template.Title,
                template.Subject,
                template.Content,
                currentUser.UserId,
                currentUser.UserName,
                DateTimeOffset.UtcNow,
                Guid.CreateVersion7()));

            return Results.Ok(new MessageSentResponse(true, "Mensaje enviado correctamente."));
        })
        .Produces<MessageSentResponse>(200)
        .Produces<ErrorResponse>(404);
    }
}
