namespace SA.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        Guid userId,
        Guid tenantId,
        string username,
        Guid? entityId,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions);

    string GenerateRefreshToken();
}
