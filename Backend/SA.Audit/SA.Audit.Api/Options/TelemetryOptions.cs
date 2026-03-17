namespace SA.Audit.Api.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; set; } = "sa-audit";
    public string? OtlpEndpoint { get; set; }
}
