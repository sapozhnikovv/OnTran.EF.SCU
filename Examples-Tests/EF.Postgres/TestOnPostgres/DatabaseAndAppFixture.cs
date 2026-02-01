using EF.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace TestOnPostgres;

public class DatabaseAndAppFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgreSqlContainer { get; private set; }
    public ServiceProvider ServiceProvider { get; private set; }
    public AsyncServiceScope Scope { get; private set; }
    public PostgresContext DB { get; private set; }
    public async Task InitializeAsync()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15.1")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        await PostgreSqlContainer.StartAsync();
        while (PostgreSqlContainer.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        var services = new ServiceCollection().AddDbContext<PostgresContext>(options => options.UseNpgsql(PostgreSqlContainer.GetConnectionString()));
        ServiceProvider = services.BuildServiceProvider();
        Scope = ServiceProvider.CreateAsyncScope();
        DB = Scope.ServiceProvider.GetRequiredService<PostgresContext>();
        await DB.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Scope.DisposeAsync();
        await PostgreSqlContainer.DisposeAsync();
    }
}
