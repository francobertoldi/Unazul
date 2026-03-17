using Shared.Contract.Enums;

namespace SA.Config.Api.ViewModels.ExternalServices;

public sealed record ExternalServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    ServiceType Type,
    string BaseUrl,
    ServiceStatus Status,
    AuthType AuthType,
    int TimeoutMs,
    int MaxRetries,
    DateTime? LastTestedAt,
    bool? LastTestSuccess);
