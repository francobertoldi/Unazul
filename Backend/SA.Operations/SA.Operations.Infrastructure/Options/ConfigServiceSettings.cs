namespace SA.Operations.Infrastructure.Options;

public sealed class ConfigServiceSettings
{
    public const string SectionName = "ConfigService";

    public string BaseUrl { get; set; } = string.Empty;
}
