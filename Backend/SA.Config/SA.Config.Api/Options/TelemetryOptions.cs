namespace SA.Config.Api.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; set; } = "SA.Config";
    public string? OtlpEndpoint { get; set; }
}
