using Mediator;
using SA.Config.Application.Dtos.ExternalServices;

namespace SA.Config.Application.Commands.ExternalServices;

public readonly record struct UpdateExternalServiceCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? Type,
    string? BaseUrl,
    string? Status,
    int? TimeoutMs,
    int? MaxRetries,
    string? AuthType,
    AuthConfigInput[]? AuthConfigs,
    Guid UpdatedBy) : ICommand<ExternalServiceDto>;
