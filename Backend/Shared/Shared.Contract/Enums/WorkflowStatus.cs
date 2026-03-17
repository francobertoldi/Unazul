using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum WorkflowStatus
{
    [EnumMember(Value = "draft")]
    Draft,

    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "inactive")]
    Inactive
}
