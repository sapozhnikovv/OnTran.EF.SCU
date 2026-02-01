using EF.MsSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace TestOnMsSql;

public class DatabaseAndAppFixture : IAsyncLifetime
{
    public MsSqlContainer MsSqlContainer { get; private set; }
    public ServiceProvider ServiceProvider { get; private set; }
    public AsyncServiceScope Scope { get; private set; }
    public MsSqlContext DB { get; private set; }
    public async Task InitializeAsync()
    {
        MsSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
            .Build();
        await MsSqlContainer.StartAsync();
        while (MsSqlContainer.State != DotNet.Testcontainers.Containers.TestcontainersStates.Running)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        var services = new ServiceCollection().AddDbContext<MsSqlContext>(options => options.UseSqlServer(MsSqlContainer.GetConnectionString()));
        ServiceProvider = services.BuildServiceProvider();
        Scope = ServiceProvider.CreateAsyncScope();
        DB = Scope.ServiceProvider.GetRequiredService<MsSqlContext>();
        await DB.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Scope.DisposeAsync();
        await MsSqlContainer.DisposeAsync();
    }
}
