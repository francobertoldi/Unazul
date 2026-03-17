using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum AuditOperationType
{
    [EnumMember(Value = "crear")]
    Crear,

    [EnumMember(Value = "editar")]
    Editar,

    [EnumMember(Value = "eliminar")]
    Eliminar,

    [EnumMember(Value = "login")]
    Login,

    [EnumMember(Value = "logout")]
    Logout,

    [EnumMember(Value = "cambiar_contrasena")]
    CambiarContrasena,

    [EnumMember(Value = "cambiar_estado")]
    CambiarEstado,

    [EnumMember(Value = "liquidar")]
    Liquidar,

    [EnumMember(Value = "exportar")]
    Exportar,

    [EnumMember(Value = "consultar")]
    Consultar,

    [EnumMember(Value = "otro")]
    Otro
}
