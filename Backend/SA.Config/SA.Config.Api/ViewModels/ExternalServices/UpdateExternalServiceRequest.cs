namespace SA.Config.Api.ViewModels.ExternalServices;

public sealed record UpdateExternalServiceRequest(
    string? Name,
    string? Description,
    string? Type,
    string? BaseUrl,
    string? Status,
    int? TimeoutMs,
    int? MaxRetries,
    string? AuthType,
    AuthConfigRequest[]? AuthConfigs);
