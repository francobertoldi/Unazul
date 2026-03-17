using Mediator;

namespace SA.Config.Application.Commands.NotificationTemplates;

public readonly record struct DeleteNotificationTemplateCommand(Guid Id) : ICommand;
