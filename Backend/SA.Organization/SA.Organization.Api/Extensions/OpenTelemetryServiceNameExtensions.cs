using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SA.Organization.Api.Options;

namespace SA.Organization.Api.Extensions;

public static class OpenTelemetryServiceNameExtensions
{
    public static IServiceCollection AddOrganizationOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var telemetry = configuration.GetSection(TelemetryOptions.SectionName).Get<TelemetryOptions>()
            ?? new TelemetryOptions();

        if (string.IsNullOrWhiteSpace(telemetry.OtlpEndpoint))
        {
            return services;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(telemetry.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opts => opts.Endpoint = new Uri(telemetry.OtlpEndpoint));
            });

        return services;
    }
}
