namespace SA.Config.Application.Dtos.ExternalServices;

public sealed record TestResultDto(
    bool Success,
    long ResponseTimeMs,
    string? Error);
