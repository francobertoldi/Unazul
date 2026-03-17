namespace SA.Identity.Domain.Entities;

public sealed class UserAssignment
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ScopeType { get; private set; } = string.Empty;
    public Guid ScopeId { get; private set; }
    public string ScopeName { get; private set; } = string.Empty;

    public User User { get; private set; } = null!;

    private UserAssignment() { }

    public static UserAssignment Create(Guid userId, string scopeType, Guid scopeId, string scopeName)
    {
        return new UserAssignment
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            ScopeType = scopeType,
            ScopeId = scopeId,
            ScopeName = scopeName
        };
    }
}
