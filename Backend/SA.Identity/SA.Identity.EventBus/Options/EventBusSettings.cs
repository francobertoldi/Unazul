namespace SA.Identity.EventBus.Options;

public sealed class EventBusSettings
{
    public const string SectionName = "EventBusSettings";

    public string HostAddress { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
