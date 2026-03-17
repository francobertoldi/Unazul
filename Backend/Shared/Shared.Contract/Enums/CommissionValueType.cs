using System.Runtime.Serialization;

namespace Shared.Contract.Enums;

public enum CommissionValueType
{
    [EnumMember(Value = "fixed_per_sale")]
    FixedPerSale,

    [EnumMember(Value = "percentage_capital")]
    PercentageCapital,

    [EnumMember(Value = "percentage_total_loan")]
    PercentageTotalLoan
}
