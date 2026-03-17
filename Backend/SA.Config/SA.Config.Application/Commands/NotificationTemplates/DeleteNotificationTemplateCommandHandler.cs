using Mediator;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.NotificationTemplates;

public sealed class DeleteNotificationTemplateCommandHandler(
    INotificationTemplateRepository templateRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<DeleteNotificationTemplateCommand>
{
    public async ValueTask<Unit> Handle(DeleteNotificationTemplateCommand command, CancellationToken ct)
    {
        var template = await templateRepository.GetByIdAsync(command.Id, ct);
        if (template is null)
        {
            throw new NotFoundException("NTPL_NOT_FOUND", "Plantilla de notificacion no encontrada.");
        }

        var isReferenced = await templateRepository.IsReferencedByActiveWorkflowAsync(template.Id, ct);
        if (isReferenced)
        {
            throw new ConflictException("NTPL_REFERENCED_BY_WORKFLOW", "La plantilla esta referenciada por un workflow activo y no puede eliminarse.");
        }

        var tenantId = template.TenantId;
        var code = template.Code;
        var channel = template.Channel;

        await templateRepository.DeleteAsync(template, ct);
        await templateRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new NotificationTemplateDeletedEvent(
            TenantId: tenantId,
            TemplateId: template.Id,
            Code: code,
            Channel: channel,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return Unit.Value;
    }
}
