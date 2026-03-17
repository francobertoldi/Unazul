namespace SA.Catalog.Application.Dtos;

public sealed record InvestmentAttributesDto(
    decimal MinimumAmount, decimal? ExpectedReturn,
    int? TermDays, string RiskLevel);
