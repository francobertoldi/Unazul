namespace SA.Identity.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "JwtOptions";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SA.Identity";
    public string Audience { get; set; } = "SA.Unazul";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
