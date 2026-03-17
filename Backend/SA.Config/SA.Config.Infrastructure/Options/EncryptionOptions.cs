namespace SA.Config.Infrastructure.Options;

public sealed class EncryptionOptions
{
    public const string SectionName = "EncryptionOptions";

    /// <summary>
    /// AES-256 key. Must be exactly 32 ASCII characters (256 bits).
    /// </summary>
    public string Key { get; set; } = string.Empty;
}
