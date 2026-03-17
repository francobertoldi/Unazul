using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum UserStatus
{
    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "inactive")]
    Inactive,

    [EnumMember(Value = "locked")]
    Locked
}
