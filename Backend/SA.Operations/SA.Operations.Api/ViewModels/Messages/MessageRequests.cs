using SA.Operations.Domain.Enums;

namespace SA.Operations.Api.ViewModels.Messages;

public sealed record SendMessageRequest(
    Guid TemplateId,
    MessageChannel Channel,
    string Recipient,
    Dictionary<string, string>? Variables);
