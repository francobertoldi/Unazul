namespace SA.Operations.Api.Options;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public string ServiceName { get; set; } = "SA.Operations";
    public string? OtlpEndpoint { get; set; }
}
