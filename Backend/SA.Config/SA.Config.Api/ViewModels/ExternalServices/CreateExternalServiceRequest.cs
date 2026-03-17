namespace SA.Config.Api.ViewModels.ExternalServices;

public sealed record CreateExternalServiceRequest(
    string Name,
    string? Description,
    string Type,
    string BaseUrl,
    string? Status,
    int? TimeoutMs,
    int? MaxRetries,
    string AuthType,
    AuthConfigRequest[]? AuthConfigs);

public sealed record AuthConfigRequest(string Key, string Value);
