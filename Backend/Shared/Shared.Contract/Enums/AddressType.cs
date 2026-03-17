using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum AddressType
{
    [EnumMember(Value = "home")]
    Home,

    [EnumMember(Value = "work")]
    Work,

    [EnumMember(Value = "legal")]
    Legal,

    [EnumMember(Value = "other")]
    Other
}
