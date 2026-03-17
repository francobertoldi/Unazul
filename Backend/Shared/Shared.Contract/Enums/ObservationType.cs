using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ObservationType
{
    [EnumMember(Value = "manual")]
    Manual,

    [EnumMember(Value = "message")]
    Message
}
