namespace SA.Catalog.Application.Dtos;

public sealed record ProductRequirementDto(
    Guid Id, string Name, string Type, bool IsMandatory, string? Description);
