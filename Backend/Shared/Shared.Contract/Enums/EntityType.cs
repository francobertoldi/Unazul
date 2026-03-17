using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum EntityType
{
    [EnumMember(Value = "bank")]
    Bank,

    [EnumMember(Value = "insurance")]
    Insurance,

    [EnumMember(Value = "fintech")]
    Fintech,

    [EnumMember(Value = "cooperative")]
    Cooperative,

    [EnumMember(Value = "sgr")]
    Sgr,

    [EnumMember(Value = "regional_card")]
    RegionalCard
}
