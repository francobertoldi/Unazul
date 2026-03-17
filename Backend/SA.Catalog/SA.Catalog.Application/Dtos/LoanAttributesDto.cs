namespace SA.Catalog.Application.Dtos;

public sealed record LoanAttributesDto(
    string AmortizationType, decimal AnnualEffectiveRate,
    decimal? CftRate, decimal? AdminFees);
