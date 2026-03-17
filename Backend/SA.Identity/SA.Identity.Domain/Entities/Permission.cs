namespace SA.Identity.Domain.Entities;

public sealed class Permission
{
    public Guid Id { get; private set; }
    public string Module { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Permission() { }

    public static Permission Create(string module, string action, string code, string? description)
    {
        return new Permission
        {
            Id = Guid.CreateVersion7(),
            Module = module,
            Action = action,
            Code = code,
            Description = description
        };
    }
}
