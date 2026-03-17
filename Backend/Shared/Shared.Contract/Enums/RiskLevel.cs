using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum RiskLevel
{
    [EnumMember(Value = "low")]
    Low,

    [EnumMember(Value = "medium")]
    Medium,

    [EnumMember(Value = "high")]
    High
}
