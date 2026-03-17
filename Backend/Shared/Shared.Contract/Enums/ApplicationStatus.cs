using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ApplicationStatus
{
    [EnumMember(Value = "draft")]
    Draft,

    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "in_review")]
    InReview,

    [EnumMember(Value = "approved")]
    Approved,

    [EnumMember(Value = "rejected")]
    Rejected,

    [EnumMember(Value = "cancelled")]
    Cancelled,

    [EnumMember(Value = "settled")]
    Settled
}
