namespace Shared.Auth;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string UserName { get; }
    string? IpAddress { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permissionCode);
}
