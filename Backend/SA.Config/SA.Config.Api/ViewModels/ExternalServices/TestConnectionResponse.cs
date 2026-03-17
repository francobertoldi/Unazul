namespace SA.Config.Api.ViewModels.ExternalServices;

public sealed record TestConnectionResponse(
    bool Success,
    long ResponseTimeMs,
    string? Error);
