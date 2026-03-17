using Shared.Contract.Enums;

namespace SA.Config.Domain.Entities;

public sealed class ExternalService
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ServiceType Type { get; private set; }
    public string BaseUrl { get; private set; } = string.Empty;
    public ServiceStatus Status { get; private set; }
    public int TimeoutMs { get; private set; }
    public int MaxRetries { get; private set; }
    public AuthType AuthType { get; private set; }
    public DateTime? LastTestedAt { get; private set; }
    public bool? LastTestSuccess { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public ICollection<ServiceAuthConfig> AuthConfigs { get; private set; } = [];

    private ExternalService() { }

    public static ExternalService Create(
        Guid tenantId,
        string name,
        string? description,
        ServiceType type,
        string baseUrl,
        ServiceStatus status,
        int timeoutMs,
        int maxRetries,
        AuthType authType,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new ExternalService
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            Type = type,
            BaseUrl = baseUrl,
            Status = status,
            TimeoutMs = timeoutMs,
            MaxRetries = maxRetries,
            AuthType = authType,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void Update(
        string name,
        string? description,
        ServiceType type,
        string baseUrl,
        ServiceStatus status,
        int timeoutMs,
        int maxRetries,
        AuthType authType,
        Guid updatedBy)
    {
        Name = name;
        Description = description;
        Type = type;
        BaseUrl = baseUrl;
        Status = status;
        TimeoutMs = timeoutMs;
        MaxRetries = maxRetries;
        AuthType = authType;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordTestResult(bool success, DateTime now)
    {
        LastTestedAt = now;
        LastTestSuccess = success;
        UpdatedAt = now;
    }

    public bool IsActive => Status == ServiceStatus.Active;
    public bool IsInactive => Status == ServiceStatus.Inactive;
    public bool HasError => Status == ServiceStatus.Error;
}
