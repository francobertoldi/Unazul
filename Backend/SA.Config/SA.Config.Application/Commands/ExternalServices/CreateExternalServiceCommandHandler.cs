using Mediator;
using SA.Config.Application.Dtos.ExternalServices;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using SA.Config.Domain.Entities;
using Shared.Contract.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.ExternalServices;

public sealed class CreateExternalServiceCommandHandler(
    IExternalServiceRepository externalServiceRepository,
    IEncryptionService encryptionService,
    IIntegrationEventPublisher eventPublisher) : ICommandHandler<CreateExternalServiceCommand, ExternalServiceDto>
{
    private static readonly Dictionary<AuthType, string[]> RequiredAuthKeys = new()
    {
        [AuthType.ApiKey] = ["header_name", "api_key"],
        [AuthType.BearerToken] = ["token"],
        [AuthType.BasicAuth] = ["username", "password"],
        [AuthType.OAuth2] = ["client_id", "client_secret", "token_url"],
        [AuthType.CustomHeader] = ["header_name", "header_value"],
        [AuthType.None] = []
    };

    public async ValueTask<ExternalServiceDto> Handle(CreateExternalServiceCommand command, CancellationToken ct)
    {
        var exists = await externalServiceRepository.ExistsByNameAsync(command.TenantId, command.Name, ct: ct);
        if (exists)
        {
            throw new ConflictException("CFG_DUPLICATE_SERVICE_NAME", "El nombre del servicio ya existe.");
        }

        ValidateAuthConfigs(command.AuthType, command.AuthConfigs);

        var serviceType = Enum.Parse<ServiceType>(command.Type, ignoreCase: true);
        var serviceStatus = command.Status is not null
            ? Enum.Parse<ServiceStatus>(command.Status, ignoreCase: true)
            : ServiceStatus.Active;
        var authType = Enum.Parse<AuthType>(command.AuthType, ignoreCase: true);

        var service = ExternalService.Create(
            command.TenantId,
            command.Name,
            command.Description,
            serviceType,
            command.BaseUrl,
            serviceStatus,
            command.TimeoutMs ?? 30000,
            command.MaxRetries ?? 3,
            authType,
            command.CreatedBy);

        await externalServiceRepository.AddAsync(service, ct);

        if (command.AuthConfigs is { Length: > 0 })
        {
            var configs = command.AuthConfigs
                .Select(c => ServiceAuthConfig.Create(
                    service.Id,
                    c.Key,
                    encryptionService.Encrypt(c.Value)))
                .ToList();

            await externalServiceRepository.ReplaceAuthConfigsAsync(service.Id, configs, ct);
            await externalServiceRepository.SaveChangesAsync(ct);
        }

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: command.TenantId,
            UserId: command.CreatedBy,
            UserName: string.Empty,
            Operation: "WRITE",
            Module: "config",
            Action: "service_created",
            Detail: $"External service '{command.Name}' created",
            IpAddress: null,
            EntityType: "ExternalService",
            EntityId: service.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return new ExternalServiceDto(
            service.Id,
            service.Name,
            service.Description,
            service.Type,
            service.BaseUrl,
            service.Status,
            service.AuthType,
            service.TimeoutMs,
            service.MaxRetries,
            service.LastTestedAt,
            service.LastTestSuccess);
    }

    private static void ValidateAuthConfigs(string authType, AuthConfigInput[]? authConfigs)
    {
        if (!Enum.TryParse<AuthType>(authType, ignoreCase: true, out var parsedAuthType) ||
            !RequiredAuthKeys.TryGetValue(parsedAuthType, out var requiredKeys))
        {
            throw new ValidationException("CFG_INVALID_AUTH_CONFIG", "Configuracion de autenticacion invalida para el tipo especificado.");
        }

        if (parsedAuthType == AuthType.None)
        {
            return;
        }

        if (authConfigs is null || authConfigs.Length == 0)
        {
            throw new ValidationException("CFG_INVALID_AUTH_CONFIG", "Configuracion de autenticacion invalida para el tipo especificado.");
        }

        var providedKeys = authConfigs.Select(c => c.Key.ToLowerInvariant()).ToHashSet();

        foreach (var key in requiredKeys)
        {
            if (!providedKeys.Contains(key))
            {
                throw new ValidationException("CFG_INVALID_AUTH_CONFIG", "Configuracion de autenticacion invalida para el tipo especificado.");
            }
        }
    }
}
