using EF.MySql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MySql;

namespace TestOnMySql;

public class DatabaseAndAppFixture : IAsyncLifetime
{
    public MySqlContainer MySqlContainer { get; private set; }
    public ServiceProvider ServiceProvider { get; private set; }
    public AsyncServiceScope Scope { get; private set; }
    public MySqlContext DB { get; private set; }
    public async Task InitializeAsync()
    {
        MySqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        await MySqlContainer.StartAsync();
        while (MySqlContainer.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        var services = new ServiceCollection().AddDbContext<MySqlContext>(options => options.UseMySQL(MySqlContainer.GetConnectionString()));
        ServiceProvider = services.BuildServiceProvider();
        Scope = ServiceProvider.CreateAsyncScope();
        DB = Scope.ServiceProvider.GetRequiredService<MySqlContext>();
        await DB.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Scope.DisposeAsync();
        await MySqlContainer.DisposeAsync();
    }
}
