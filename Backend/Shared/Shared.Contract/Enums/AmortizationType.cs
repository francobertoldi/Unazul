using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum AmortizationType
{
    [EnumMember(Value = "french")]
    French,

    [EnumMember(Value = "german")]
    German,

    [EnumMember(Value = "american")]
    American,

    [EnumMember(Value = "bullet")]
    Bullet
}
