namespace SA.Identity.Domain.ValueObjects;

public sealed record PasswordHash
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Password hash cannot be empty.", nameof(value));

        Value = value;
    }

    public static PasswordHash From(string hash) => new(hash);

    public override string ToString() => "***";
}
