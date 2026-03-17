namespace SA.Identity.Api.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; set; } = "SA.Identity";
    public string? OtlpEndpoint { get; set; }
}
