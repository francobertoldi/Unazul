using Mediator;

namespace SA.Operations.Application.Commands.Addresses;

public readonly record struct UpdateAddressCommand(
    Guid AddressId,
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
    decimal? Longitude) : ICommand<UpdateAddressResult>;

public sealed record UpdateAddressResult(Guid Id, string Type);
