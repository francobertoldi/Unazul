using SA.Operations.Domain.Enums;

namespace SA.Operations.Domain.Entities;

public sealed class ApplicationObservation
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid TenantId { get; private set; }
    public ObservationType ObservationType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private ApplicationObservation() { }

    public static ApplicationObservation Create(
        Guid applicationId,
        Guid tenantId,
        ObservationType observationType,
        string content,
        Guid userId,
        string userName)
    {
        return new ApplicationObservation
        {
            Id = Guid.CreateVersion7(),
            ApplicationId = applicationId,
            TenantId = tenantId,
            ObservationType = observationType,
            Content = content,
            UserId = userId,
            UserName = userName,
            CreatedAt = DateTime.UtcNow
        };
    }
}
