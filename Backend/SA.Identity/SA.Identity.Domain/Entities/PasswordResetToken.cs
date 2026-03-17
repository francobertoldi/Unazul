namespace SA.Identity.Domain.Entities;

public sealed class PasswordResetToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool Used { get; private set; }
    public const int TtlMinutes = 30;

    private PasswordResetToken() { }

    public static PasswordResetToken Create(Guid userId, string tokenHash)
    {
        return new PasswordResetToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TtlMinutes),
            Used = false
        };
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !Used && !IsExpired;
    public void MarkAsUsed() { Used = true; }
}
