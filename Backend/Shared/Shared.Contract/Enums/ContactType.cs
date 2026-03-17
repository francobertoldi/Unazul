using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ContactType
{
    [EnumMember(Value = "personal")]
    Personal,

    [EnumMember(Value = "work")]
    Work,

    [EnumMember(Value = "emergency")]
    Emergency,

    [EnumMember(Value = "other")]
    Other
}
