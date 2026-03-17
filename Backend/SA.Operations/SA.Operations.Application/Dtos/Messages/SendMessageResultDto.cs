namespace SA.Operations.Application.Dtos.Messages;

public sealed record SendMessageResultDto(
    Guid MessageId,
    string Channel,
    string Recipient,
    string Status);
