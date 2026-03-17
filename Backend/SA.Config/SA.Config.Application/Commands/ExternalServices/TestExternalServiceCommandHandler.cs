using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Mediator;
using SA.Config.Application.Dtos.ExternalServices;
using SA.Config.Application.Interfaces;
using SA.Config.DataAccess.Interface.Repositories;
using Shared.Contract.Enums;
using Shared.Contract.Events;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.ExternalServices;

public sealed class TestExternalServiceCommandHandler(
    IExternalServiceRepository externalServiceRepository,
    IEncryptionService encryptionService,
    IIntegrationEventPublisher eventPublisher,
    IHttpClientFactory httpClientFactory) : ICommandHandler<TestExternalServiceCommand, TestResultDto>
{
    private const string GraphQlIntrospectionQuery = """{"query":"{ __schema { queryType { name } } }"}""";

    public async ValueTask<TestResultDto> Handle(TestExternalServiceCommand command, CancellationToken ct)
    {
        var service = await externalServiceRepository.GetByIdWithAuthConfigsAsync(command.Id, ct);
        if (service is null)
        {
            throw new NotFoundException("CFG_SERVICE_NOT_FOUND", "Servicio externo no encontrado.");
        }

        var decryptedConfigs = service.AuthConfigs
            .ToDictionary(c => c.Key, c => encryptionService.Decrypt(c.ValueEncrypted), StringComparer.OrdinalIgnoreCase);

        var stopwatch = Stopwatch.StartNew();
        bool success;
        string? error = null;

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMilliseconds(service.TimeoutMs);

            var request = BuildRequest(service.Type, service.BaseUrl);
            ApplyAuth(request, service.AuthType, decryptedConfigs);

            var response = await client.SendAsync(request, ct);
            stopwatch.Stop();

            success = response.IsSuccessStatusCode;
            if (!success)
            {
                error = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            success = false;
            error = "Connection timed out";
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            success = false;
            error = ex.Message;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            success = false;
            error = ex.Message;
        }

        var now = DateTime.UtcNow;
        service.RecordTestResult(success, now);

        if (success && service.HasError)
        {
            service.Update(
                service.Name,
                service.Description,
                service.Type,
                service.BaseUrl,
                ServiceStatus.Active,
                service.TimeoutMs,
                service.MaxRetries,
                service.AuthType,
                service.UpdatedBy);
        }
        else if (!success)
        {
            service.Update(
                service.Name,
                service.Description,
                service.Type,
                service.BaseUrl,
                ServiceStatus.Error,
                service.TimeoutMs,
                service.MaxRetries,
                service.AuthType,
                service.UpdatedBy);
        }

        externalServiceRepository.Update(service);
        await externalServiceRepository.SaveChangesAsync(ct);

        await eventPublisher.PublishAsync(new DomainEvent(
            TenantId: service.TenantId,
            UserId: Guid.Empty,
            UserName: string.Empty,
            Operation: "READ",
            Module: "config",
            Action: "service_tested",
            Detail: $"External service '{service.Name}' test: {(success ? "success" : "failed")}",
            IpAddress: null,
            EntityType: "ExternalService",
            EntityId: service.Id,
            ChangesJson: null,
            OccurredAt: DateTimeOffset.UtcNow,
            CorrelationId: Guid.CreateVersion7()), ct);

        return new TestResultDto(success, stopwatch.ElapsedMilliseconds, error);
    }

    private static HttpRequestMessage BuildRequest(ServiceType type, string baseUrl)
    {
        return type switch
        {
            ServiceType.RestApi => new HttpRequestMessage(HttpMethod.Head, baseUrl),
            ServiceType.GraphQl => new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(GraphQlIntrospectionQuery, Encoding.UTF8, "application/json")
            },
            ServiceType.Soap => new HttpRequestMessage(HttpMethod.Get, baseUrl.TrimEnd('/') + "?wsdl"),
            ServiceType.Webhook => new HttpRequestMessage(HttpMethod.Head, baseUrl),
            ServiceType.Mcp => new HttpRequestMessage(HttpMethod.Get, baseUrl),
            _ => new HttpRequestMessage(HttpMethod.Head, baseUrl)
        };
    }

    private static void ApplyAuth(HttpRequestMessage request, AuthType authType, Dictionary<string, string> configs)
    {
        switch (authType)
        {
            case AuthType.ApiKey:
                if (configs.TryGetValue("header_name", out var headerName) &&
                    configs.TryGetValue("api_key", out var apiKey))
                {
                    request.Headers.TryAddWithoutValidation(headerName, apiKey);
                }
                break;

            case AuthType.BearerToken:
                if (configs.TryGetValue("token", out var token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                break;

            case AuthType.BasicAuth:
                if (configs.TryGetValue("username", out var username) &&
                    configs.TryGetValue("password", out var password))
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case AuthType.OAuth2:
                // For testing, we just use client_id/client_secret as-is
                // A full OAuth2 flow would request a token first
                if (configs.TryGetValue("client_id", out var clientId))
                {
                    request.Headers.TryAddWithoutValidation("X-Client-Id", clientId);
                }
                break;

            case AuthType.CustomHeader:
                if (configs.TryGetValue("header_name", out var customHeaderName) &&
                    configs.TryGetValue("header_value", out var headerValue))
                {
                    request.Headers.TryAddWithoutValidation(customHeaderName, headerValue);
                }
                break;

            case AuthType.None:
            default:
                break;
        }
    }
}
