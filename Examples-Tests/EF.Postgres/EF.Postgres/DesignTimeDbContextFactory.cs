using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EF.Postgres;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PostgresContext>
{
    public PostgresContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgresContext>();
        var connectionString = "Host=localhost;Port=5432;Database=mydatabase;Username=postgres;Password=123;";
        optionsBuilder.UseNpgsql(connectionString);
        return new PostgresContext(optionsBuilder.Options);
    }
}