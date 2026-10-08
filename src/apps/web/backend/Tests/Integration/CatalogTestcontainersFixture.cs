using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace Tests.Integration;

[SetUpFixture]
[Explicit("Requiere Docker. No ejecutable en sandbox.")]
public class CatalogTestcontainersFixture
{
    private static IContainer? _container;

    [OneTimeSetUp]
    public static async Task StartSqlServer()
    {
        _container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_SA_PASSWORD", "Local_Dev_Only_ChangeMe")
            .WithPortBinding(1433, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Recovery is complete"))
            .Build();

        await _container.StartAsync();
    }

    [OneTimeTearDown]
    public static async Task StopSqlServer()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
