using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum ParameterType
{
    [EnumMember(Value = "text")]
    Text,

    [EnumMember(Value = "number")]
    Number,

    [EnumMember(Value = "boolean")]
    Boolean,

    [EnumMember(Value = "select")]
    Select,

    [EnumMember(Value = "list")]
    List,

    [EnumMember(Value = "html")]
    Html
}
