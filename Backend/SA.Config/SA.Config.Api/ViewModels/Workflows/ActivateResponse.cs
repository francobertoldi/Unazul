namespace SA.Config.Api.ViewModels.Workflows;

public sealed record ActivateResponse(
    Guid Id,
    string Name,
    string Status,
    int Version,
    DateTime CreatedAt);
