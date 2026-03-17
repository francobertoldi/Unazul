using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum TenantStatus
{
    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "inactive")]
    Inactive,

    [EnumMember(Value = "suspended")]
    Suspended
}
