namespace SA.Operations.Infrastructure.Options;

public sealed class CatalogServiceSettings
{
    public const string SectionName = "CatalogService";

    public string BaseUrl { get; set; } = string.Empty;
}
