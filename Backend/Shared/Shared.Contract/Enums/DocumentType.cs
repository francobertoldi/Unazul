using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum DocumentType
{
    [EnumMember(Value = "dni")]
    Dni,

    [EnumMember(Value = "cuit")]
    Cuit,

    [EnumMember(Value = "passport")]
    Passport
}
