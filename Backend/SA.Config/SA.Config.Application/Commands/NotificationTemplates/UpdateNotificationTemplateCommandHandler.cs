using Mediator;
using SA.Config.Application.Dtos.NotificationTemplates;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.NotificationTemplates;

public sealed class UpdateNotificationTemplateCommandHandler(
    INotificationTemplateRepository templateRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<UpdateNotificationTemplateCommand, NotificationTemplateDetailDto>
{
    public async ValueTask<NotificationTemplateDetailDto> Handle(UpdateNotificationTemplateCommand command, CancellationToken ct)
    {
        var template = await templateRepository.GetByIdAsync(command.Id, ct);
        if (template is null)
        {
            throw new NotFoundException("NTPL_NOT_FOUND", "Plantilla de notificacion no encontrada.");
        }

        if (template.Channel == "email" && string.IsNullOrWhiteSpace(command.Subject))
        {
            throw new ValidationException("NTPL_SUBJECT_REQUIRED_FOR_EMAIL", "El asunto es obligatorio para plantillas de email.");
        }

        var status = string.IsNullOrWhiteSpace(command.Status) ? template.Status : command.Status;

        template.Update(command.Name, command.Subject, command.Body, status, command.UpdatedBy);

        templateRepository.Update(template);
        await templateRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new NotificationTemplateUpdatedEvent(
            TenantId: template.TenantId,
            TemplateId: template.Id,
            Code: template.Code,
            Channel: template.Channel,
            UpdatedBy: command.UpdatedBy,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return CreateNotificationTemplateCommandHandler.MapToDetailDto(template);
    }
}
