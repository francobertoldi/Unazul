namespace SA.Config.Domain.Entities;

public sealed class NotificationTemplate
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Channel { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public string Status { get; private set; } = "active";
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        Guid tenantId,
        string code,
        string name,
        string channel,
        string? subject,
        string body,
        string status,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new NotificationTemplate
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Code = code,
            Name = name,
            Channel = channel,
            Subject = subject,
            Body = body,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void Update(string name, string? subject, string body, string status, Guid updatedBy)
    {
        Name = name;
        Subject = subject;
        Body = body;
        Status = status;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == "active";
    public bool IsInactive => Status == "inactive";
}
