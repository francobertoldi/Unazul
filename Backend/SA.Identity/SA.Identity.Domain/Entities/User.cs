using Shared.Contract.Enums;

namespace SA.Identity.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? EntityName { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public string? Avatar { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid UpdatedBy { get; private set; }

    public ICollection<UserAssignment> Assignments { get; private set; } = [];
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private User() { }

    public static User Create(
        Guid tenantId,
        string username,
        string passwordHash,
        string email,
        string firstName,
        string lastName,
        Guid? entityId,
        string? entityName,
        Guid createdBy)
    {
        var now = DateTime.UtcNow;
        return new User
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EntityId = entityId,
            EntityName = entityName,
            Status = UserStatus.Active,
            FailedLoginAttempts = 0,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
    }

    public void Update(
        string email,
        string firstName,
        string lastName,
        Guid? entityId,
        string? entityName,
        UserStatus status,
        string? avatar,
        Guid updatedBy)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        EntityId = entityId;
        EntityName = entityName;
        Status = status;
        Avatar = avatar;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            Status = UserStatus.Locked;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LastLogin = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash, Guid updatedBy)
    {
        PasswordHash = newPasswordHash;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLocked => Status == UserStatus.Locked;
    public bool IsActive => Status == UserStatus.Active;
    public bool IsInactive => Status == UserStatus.Inactive;
}
