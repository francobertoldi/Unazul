using Mediator;
using SA.Operations.Application.Dtos.Messages;
using SA.Operations.Application.Interfaces;
using SA.Operations.DataAccess.Interface.Repositories;
using SA.Operations.Domain.Entities;
using SA.Operations.Domain.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Operations.Application.Commands.Messages;

public sealed class SendMessageCommandHandler(
    IApplicationRepository applicationRepository,
    IObservationRepository observationRepository,
    IConfigServiceClient configClient,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<SendMessageCommand, SendMessageResultDto>
{
    public async ValueTask<SendMessageResultDto> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var app = await applicationRepository.GetByIdAsync(command.ApplicationId, ct);
        if (app is null || app.TenantId != command.TenantId)
            throw new NotFoundException("OPS_APPLICATION_NOT_FOUND", "Solicitud no encontrada.");

        // Fetch template from Config service
        var template = await configClient.GetNotificationTemplateAsync(command.TemplateId, ct);
        if (template is null)
            throw new NotFoundException("OPS_TEMPLATE_NOT_FOUND", "Plantilla de notificacion no encontrada.");

        // Resolve variables in template content
        var body = template.Content;
        var subject = template.Subject;
        if (command.Variables is { Count: > 0 })
        {
            foreach (var (key, value) in command.Variables)
            {
                body = body.Replace($"{{{{{key}}}}}", value);
                if (subject is not null)
                    subject = subject.Replace($"{{{{{key}}}}}", value);
            }
        }

        // Create observation with type=Message
        var observation = ApplicationObservation.Create(
            command.ApplicationId,
            command.TenantId,
            ObservationType.Message,
            body,
            command.SentBy,
            command.SentByName);

        await observationRepository.AddAsync(observation, ct);
        await observationRepository.SaveChangesAsync(ct);

        // Publish MessageSentEvent via integration bus
        await eventPublisher.PublishAsync(new MessageSentEvent(
            command.TenantId,
            command.ApplicationId,
            command.Channel,
            command.Recipient,
            template.Title,
            subject,
            body,
            command.SentBy,
            command.SentByName,
            DateTimeOffset.UtcNow,
            Guid.CreateVersion7()), ct);

        return new SendMessageResultDto(observation.Id, command.Channel, command.Recipient, "Sent");
    }
}
