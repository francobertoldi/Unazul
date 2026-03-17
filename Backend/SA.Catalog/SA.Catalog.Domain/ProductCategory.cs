namespace SA.Catalog.Domain;

public static class ProductCategory
{
    public const string Loan = "loan";
    public const string Insurance = "insurance";
    public const string Account = "account";
    public const string Card = "card";
    public const string Investment = "investment";

    private static readonly Dictionary<string, string> PrefixMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PREST"] = Loan,
        ["SEG"] = Insurance,
        ["CTA"] = Account,
        ["TARJETA"] = Card,
        ["INV"] = Investment
    };

    public static readonly string[] ValidPrefixes = ["PREST", "SEG", "CTA", "TARJETA", "INV"];

    public static string? GetCategoryFromCode(string code)
    {
        foreach (var kvp in PrefixMap)
        {
            if (code.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return null;
    }

    public static bool IsValidPrefix(string code)
    {
        return GetCategoryFromCode(code) is not null;
    }
}
