using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ChannelType
{
    [EnumMember(Value = "web")]
    Web,

    [EnumMember(Value = "mobile")]
    Mobile,

    [EnumMember(Value = "api")]
    Api,

    [EnumMember(Value = "presencial")]
    Presencial,

    [EnumMember(Value = "ia_agent")]
    IaAgent
}
