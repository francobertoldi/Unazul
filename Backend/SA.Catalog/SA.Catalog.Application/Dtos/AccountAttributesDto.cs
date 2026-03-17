namespace SA.Catalog.Application.Dtos;

public sealed record AccountAttributesDto(
    decimal MaintenanceFee, decimal? MinimumBalance,
    decimal? InterestRate, string AccountType);
