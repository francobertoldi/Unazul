using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum Gender
{
    [EnumMember(Value = "male")]
    Male,

    [EnumMember(Value = "female")]
    Female,

    [EnumMember(Value = "other")]
    Other,

    [EnumMember(Value = "not_specified")]
    NotSpecified
}
