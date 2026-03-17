using Mediator;
using SA.Config.Application.Dtos.NotificationTemplates;

namespace SA.Config.Application.Commands.NotificationTemplates;

public readonly record struct CreateNotificationTemplateCommand(
    Guid TenantId,
    string Code,
    string Name,
    string Channel,
    string? Subject,
    string Body,
    string? Status,
    Guid CreatedBy) : ICommand<NotificationTemplateDetailDto>;
