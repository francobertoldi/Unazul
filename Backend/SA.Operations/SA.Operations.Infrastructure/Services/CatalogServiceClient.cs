using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SA.Operations.Application.Interfaces;
using SA.Operations.Infrastructure.Options;

namespace SA.Operations.Infrastructure.Services;

public sealed class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogServiceClient> _logger;

    public CatalogServiceClient(
        IHttpClientFactory httpClientFactory,
        IOptions<CatalogServiceSettings> settings,
        ILogger<CatalogServiceClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl);
        _logger = logger;
    }

    public async Task<CatalogProductResult?> ValidateProductAndPlanAsync(
        Guid productId,
        Guid planId,
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/products/{productId}/plans/{planId}/validate", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Product/Plan validation failed: {ProductId}/{PlanId} - {Status}",
                    productId, planId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CatalogProductResult>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error validating product/plan: {ProductId}/{PlanId}",
                productId, planId);
            return null;
        }
    }

    public async Task<IReadOnlyList<CommissionPlanResult>> GetCommissionPlansAsync(
        Guid[] productIds,
        Guid[] planIds,
        CancellationToken ct = default)
    {
        try
        {
            var request = new { ProductIds = productIds, PlanIds = planIds };
            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/commissions/batch", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Commission plans batch request failed: {Status}",
                    response.StatusCode);
                return [];
            }

            var result = await response.Content
                .ReadFromJsonAsync<List<CommissionPlanResult>>(ct);

            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commission plans");
            return [];
        }
    }
}
