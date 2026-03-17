namespace SA.Config.Application.Dtos.Parameters;

public sealed record ParameterOptionDto(
    Guid Id,
    string OptionValue,
    string OptionLabel,
    int SortOrder);
