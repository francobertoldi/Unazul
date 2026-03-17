namespace SA.Operations.Domain.Entities;

public sealed class TraceEventDetail
{
    public Guid Id { get; private set; }
    public Guid TraceEventId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    private TraceEventDetail() { }

    public static TraceEventDetail Create(
        Guid traceEventId,
        Guid tenantId,
        string key,
        string value)
    {
        return new TraceEventDetail
        {
            Id = Guid.CreateVersion7(),
            TraceEventId = traceEventId,
            TenantId = tenantId,
            Key = key,
            Value = value
        };
    }
}
