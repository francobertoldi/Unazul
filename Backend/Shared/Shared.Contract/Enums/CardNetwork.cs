using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum CardNetwork
{
    [EnumMember(Value = "visa")]
    Visa,

    [EnumMember(Value = "mastercard")]
    Mastercard,

    [EnumMember(Value = "amex")]
    Amex,

    [EnumMember(Value = "cabal")]
    Cabal,

    [EnumMember(Value = "naranja")]
    Naranja
}
