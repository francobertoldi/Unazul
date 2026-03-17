using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SA.Operations.Application.Interfaces;
using SA.Operations.Infrastructure.Options;

namespace SA.Operations.Infrastructure.Services;

public sealed class ConfigServiceClient : IConfigServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConfigServiceClient> _logger;

    public ConfigServiceClient(
        IHttpClientFactory httpClientFactory,
        IOptions<ConfigServiceSettings> settings,
        ILogger<ConfigServiceClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
        _logger = logger;
    }

    public async Task<NotificationTemplateResult?> GetNotificationTemplateAsync(
        Guid templateId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/notification-templates/{templateId}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Notification template not found: {TemplateId} - {Status}",
                    templateId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<NotificationTemplateResult>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching notification template: {TemplateId}", templateId);
            return null;
        }
    }
}
