namespace SA.Organization.Api.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; set; } = "SA.Organization";
    public string? OtlpEndpoint { get; set; }
}
