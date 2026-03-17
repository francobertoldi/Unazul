using Mediator;
using SA.Operations.Application.Dtos.Messages;

namespace SA.Operations.Application.Commands.Messages;

public readonly record struct SendMessageCommand(
    Guid ApplicationId,
    Guid TenantId,
    Guid TemplateId,
    string Channel,
    string Recipient,
    Dictionary<string, string>? Variables,
    Guid SentBy,
    string SentByName) : ICommand<SendMessageResultDto>;
