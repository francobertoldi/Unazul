namespace SA.Audit.EventBus.Options;

public sealed class EventBusSettings
{
    public const string SectionName = "EventBus";

    public string HostAddress { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
