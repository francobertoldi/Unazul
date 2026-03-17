namespace SA.Operations.Api.ViewModels.Messages;

public sealed record MessageSentResponse(
    bool Success,
    string? Message);
