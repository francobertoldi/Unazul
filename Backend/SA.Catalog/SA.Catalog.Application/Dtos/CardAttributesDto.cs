namespace SA.Catalog.Application.Dtos;

public sealed record CardAttributesDto(
    decimal CreditLimit, decimal AnnualFee,
    decimal? InterestRate, string Network, string Level);
