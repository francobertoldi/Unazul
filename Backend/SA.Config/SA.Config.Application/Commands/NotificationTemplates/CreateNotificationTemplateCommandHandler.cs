using Mediator;
using SA.Config.Application.Dtos.NotificationTemplates;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.NotificationTemplates;

public sealed class CreateNotificationTemplateCommandHandler(
    INotificationTemplateRepository templateRepository,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<CreateNotificationTemplateCommand, NotificationTemplateDetailDto>
{
    private static readonly HashSet<string> ValidChannels = new(StringComparer.OrdinalIgnoreCase)
    {
        "email", "sms", "whatsapp"
    };

    public async ValueTask<NotificationTemplateDetailDto> Handle(CreateNotificationTemplateCommand command, CancellationToken ct)
    {
        var channel = command.Channel.ToLowerInvariant();

        if (!ValidChannels.Contains(channel))
        {
            throw new ValidationException("NTPL_INVALID_CHANNEL", "Canal de notificacion invalido.");
        }

        if (channel == "email" && string.IsNullOrWhiteSpace(command.Subject))
        {
            throw new ValidationException("NTPL_SUBJECT_REQUIRED_FOR_EMAIL", "El asunto es obligatorio para plantillas de email.");
        }

        var codeExists = await templateRepository.ExistsByCodeAsync(command.TenantId, command.Code, ct);
        if (codeExists)
        {
            throw new ConflictException("NTPL_DUPLICATE_CODE", "El codigo de plantilla ya existe.");
        }

        var status = string.IsNullOrWhiteSpace(command.Status) ? "active" : command.Status;

        var template = NotificationTemplate.Create(
            command.TenantId,
            command.Code,
            command.Name,
            channel,
            command.Subject,
            command.Body,
            status,
            command.CreatedBy);

        await templateRepository.AddAsync(template, ct);
        await templateRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new NotificationTemplateCreatedEvent(
            TenantId: command.TenantId,
            TemplateId: template.Id,
            Code: template.Code,
            Channel: template.Channel,
            CreatedBy: command.CreatedBy,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return MapToDetailDto(template);
    }

    internal static NotificationTemplateDetailDto MapToDetailDto(NotificationTemplate t)
    {
        return new NotificationTemplateDetailDto(
            t.Id,
            t.Code,
            t.Name,
            t.Channel,
            t.Subject,
            t.Body,
            t.Status,
            t.CreatedAt,
            t.UpdatedAt,
            t.CreatedBy,
            t.UpdatedBy);
    }
}
