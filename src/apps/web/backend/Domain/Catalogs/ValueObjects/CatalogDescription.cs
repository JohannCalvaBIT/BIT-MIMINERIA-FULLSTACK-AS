namespace Domain.Catalogs.ValueObjects;

public sealed class CatalogDescription : IEquatable<CatalogDescription>
{
    private const int MaxLength = 250;

    private CatalogDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CatalogDescription Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Catalog description cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new CatalogDescription(trimmed);
    }

    public bool Equals(CatalogDescription? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => Equals(obj as CatalogDescription);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;
}
