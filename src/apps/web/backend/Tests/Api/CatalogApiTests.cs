using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Tests.Api;

[TestFixture]
[Explicit("Requiere base de datos real para arrancar la API. No ejecutable en sandbox sin SQL Server.")]
public class CatalogApiTests
{
    [Test]
    public async Task Unauthorized_PostCompanies_Returns403()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var content = new StringContent("""{"code":"EMP001","description":"Test"}""", System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/catalogs/companies", content);

        Assert.That((int)response.StatusCode, Is.EqualTo(403));
    }
}
