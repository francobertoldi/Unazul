using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ProductStatus
{
    [EnumMember(Value = "draft")]
    Draft,

    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "inactive")]
    Inactive,

    [EnumMember(Value = "deprecated")]
    Deprecated
}
