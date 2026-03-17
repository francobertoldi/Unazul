namespace SA.Operations.Infrastructure.Options;

public sealed class StorageSettings
{
    public const string SectionName = "StorageSettings";

    public string RootPath { get; set; } = "./storage";
}
