using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EF.MsSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MsSqlContext>
{
    public MsSqlContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MsSqlContext>();
        var connectionString = "Server=localhost;Database=mydatabase;User Id=sa;Password=123;";
        optionsBuilder.UseSqlServer(connectionString);
        return new MsSqlContext(optionsBuilder.Options);
    }
}