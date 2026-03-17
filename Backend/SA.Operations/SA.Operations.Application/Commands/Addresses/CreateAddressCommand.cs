using Mediator;

namespace SA.Operations.Application.Commands.Addresses;

public readonly record struct CreateAddressCommand(
    Guid ApplicantId,
    Guid TenantId,
    string Type,
    string Street,
    string Number,
    string? Floor,
    string? Apartment,
    string City,
    string Province,
    string PostalCode,
    decimal? Latitude,
    decimal? Longitude) : ICommand<CreateAddressResult>;

public sealed record CreateAddressResult(Guid Id, string Type);
