using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum NotificationStatus
{
    [EnumMember(Value = "sent")]
    Sent,

    [EnumMember(Value = "failed")]
    Failed,

    [EnumMember(Value = "pending")]
    Pending
}
