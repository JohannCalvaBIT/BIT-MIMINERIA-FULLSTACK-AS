using Domain.Catalogs.ValueObjects;
using NUnit.Framework;

namespace Tests.Unit.Domain;

[TestFixture]
public class CatalogDescriptionTests
{
    [Test]
    public void Create_WithValidDescription_ReturnsValue()
    {
        var description = CatalogDescription.Create("Mi Minería S.A.");
        Assert.That(description.Value, Is.EqualTo("Mi Minería S.A."));
    }

    [Test]
    public void Create_WithMoreThan250Characters_Throws()
    {
        Assert.Throws<ArgumentException>(() => CatalogDescription.Create(new string('d', 251)));
    }

    [Test]
    public void Equals_IgnoresCase()
    {
        Assert.That(
            CatalogDescription.Create("minería").Equals(CatalogDescription.Create("MINERÍA")),
            Is.True);
    }
}
