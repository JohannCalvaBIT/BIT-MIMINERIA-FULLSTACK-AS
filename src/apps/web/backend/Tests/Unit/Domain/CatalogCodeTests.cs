using Domain.Catalogs.ValueObjects;
using NUnit.Framework;

namespace Tests.Unit.Domain;

[TestFixture]
public class CatalogCodeTests
{
    [Test]
    public void Create_WithValidCode_ReturnsValue()
    {
        var code = CatalogCode.Create("EMP001");
        Assert.That(code.Value, Is.EqualTo("EMP001"));
    }

    [Test]
    public void Create_TrimsWhitespace()
    {
        var code = CatalogCode.Create("  EMP001  ");
        Assert.That(code.Value, Is.EqualTo("EMP001"));
    }

    [Test]
    public void Create_WithMoreThan150Characters_Throws()
    {
        Assert.Throws<ArgumentException>(() => CatalogCode.Create(new string('a', 151)));
    }

    [Test]
    public void Create_WithWhitespaceOnly_Throws()
    {
        Assert.Throws<ArgumentException>(() => CatalogCode.Create("   "));
    }

    [Test]
    public void Equals_IgnoresCase()
    {
        Assert.That(CatalogCode.Create("emp001").Equals(CatalogCode.Create("EMP001")), Is.True);
    }
}
