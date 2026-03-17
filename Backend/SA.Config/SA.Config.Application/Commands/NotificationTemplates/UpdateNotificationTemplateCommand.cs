using Mediator;
using SA.Config.Application.Dtos.NotificationTemplates;

namespace SA.Config.Application.Commands.NotificationTemplates;

public readonly record struct UpdateNotificationTemplateCommand(
    Guid Id,
    string Name,
    string? Subject,
    string Body,
    string? Status,
    Guid UpdatedBy) : ICommand<NotificationTemplateDetailDto>;
