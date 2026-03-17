using Shared.Contract.Enums;

namespace SA.Config.Application.Dtos.ExternalServices;

public sealed record ExternalServiceDto(
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
