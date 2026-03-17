namespace SA.Audit.Domain;

public static class AuditOperationType
{
    public const string Crear = "Crear";
    public const string Editar = "Editar";
    public const string Eliminar = "Eliminar";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string CambiarContrasena = "CambiarContrasena";
    public const string CambiarEstado = "CambiarEstado";
    public const string Liquidar = "Liquidar";
    public const string Exportar = "Exportar";
    public const string Consultar = "Consultar";
    public const string Otro = "Otro";

    private static readonly HashSet<string> ValidOperations = new(StringComparer.Ordinal)
    {
        Crear, Editar, Eliminar, Login, Logout, CambiarContrasena,
        CambiarEstado, Liquidar, Exportar, Consultar, Otro
    };

    public static bool IsValid(string? operation) => operation is not null && ValidOperations.Contains(operation);
}
