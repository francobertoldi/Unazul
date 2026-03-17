namespace SA.Identity.Domain.Entities;

public sealed class Role
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private Role() { }

    public static Role Create(
        Guid tenantId,
        string name,
        string? description,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new Role
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            IsSystem = false,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void Update(string name, string? description, Guid updatedBy)
    {
        Name = name;
        Description = description;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
