using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum FlowNodeType
{
    [EnumMember(Value = "start")]
    Start,

    [EnumMember(Value = "end")]
    End,

    [EnumMember(Value = "service_call")]
    ServiceCall,

    [EnumMember(Value = "decision")]
    Decision,

    [EnumMember(Value = "send_message")]
    SendMessage,

    [EnumMember(Value = "data_capture")]
    DataCapture,

    [EnumMember(Value = "timer")]
    Timer
}
