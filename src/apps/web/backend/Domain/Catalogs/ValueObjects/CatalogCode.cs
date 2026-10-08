namespace Domain.Catalogs.ValueObjects;

public sealed class CatalogCode : IEquatable<CatalogCode>
{
    private const int MaxLength = 150;

    private CatalogCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CatalogCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Catalog code cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new CatalogCode(trimmed);
    }

    public bool Equals(CatalogCode? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as CatalogCode);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}
