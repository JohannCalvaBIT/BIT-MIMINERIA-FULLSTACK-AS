namespace Domain.Catalogs.Exceptions;

public sealed class DuplicateCodeException : Exception
{
    public DuplicateCodeException(string code)
        : base($"Code '{code}' already exists.")
    {
        Code = code;
    }

    public string Code { get; }
}

public sealed class DuplicateDescriptionException : Exception
{
    public DuplicateDescriptionException(string description)
        : base("Description already exists.")
    {
        Description = description;
    }

    public string Description { get; }
}

public sealed class CatalogInUseException : Exception
{
    public CatalogInUseException(int usageCount)
        : base($"Catalog entry is in use by {usageCount} projects.")
    {
        UsageCount = usageCount;
    }

    public int UsageCount { get; }
}
