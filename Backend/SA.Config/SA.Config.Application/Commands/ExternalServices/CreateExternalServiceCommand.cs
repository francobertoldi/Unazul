using Mediator;
using SA.Config.Application.Dtos.ExternalServices;

namespace SA.Config.Application.Commands.ExternalServices;

public readonly record struct CreateExternalServiceCommand(
    Guid TenantId,
    string Name,
    string? Description,
    string Type,
    string BaseUrl,
    string? Status,
    int? TimeoutMs,
    int? MaxRetries,
    string AuthType,
    AuthConfigInput[]? AuthConfigs,
    Guid CreatedBy) : ICommand<ExternalServiceDto>;
