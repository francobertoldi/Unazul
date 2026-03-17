using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ServiceType
{
    [EnumMember(Value = "rest_api")]
    RestApi,

    [EnumMember(Value = "mcp")]
    Mcp,

    [EnumMember(Value = "graph_ql")]
    GraphQl,

    [EnumMember(Value = "soap")]
    Soap,

    [EnumMember(Value = "webhook")]
    Webhook
}
