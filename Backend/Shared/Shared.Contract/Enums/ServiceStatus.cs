using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ServiceStatus
{
    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "inactive")]
    Inactive,

    [EnumMember(Value = "error")]
    Error
}
