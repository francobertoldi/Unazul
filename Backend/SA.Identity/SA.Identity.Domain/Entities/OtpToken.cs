namespace SA.Identity.Domain.Entities;

public sealed class OtpToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool Used { get; private set; }
    public int AttemptCount { get; private set; }
    public const int MaxAttempts = 3;
    public const int TtlMinutes = 5;
    public const int MaxResends = 3;
    public int ResendCount { get; private set; }

    private OtpToken() { }

    public static OtpToken Create(Guid userId, string codeHash)
    {
        return new OtpToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            CodeHash = codeHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(TtlMinutes),
            Used = false,
            AttemptCount = 0,
            ResendCount = 0
        };
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsMaxAttempts => AttemptCount >= MaxAttempts;
    public bool CanResend => ResendCount < MaxResends;

    public void IncrementAttempt() { AttemptCount++; }
    public void MarkAsUsed() { Used = true; }
    public void IncrementResend(string newCodeHash)
    {
        ResendCount++;
        CodeHash = newCodeHash;
        ExpiresAt = DateTime.UtcNow.AddMinutes(TtlMinutes);
        AttemptCount = 0;
    }
}
