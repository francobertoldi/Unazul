using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum DocumentStatus
{
    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "approved")]
    Approved,

    [EnumMember(Value = "rejected")]
    Rejected
}
