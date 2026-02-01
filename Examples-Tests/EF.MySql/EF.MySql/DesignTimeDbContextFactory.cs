using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EF.MySql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MySqlContext>
{
    public MySqlContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MySqlContext>();
        var connectionString = "Server=localhost;Port=3306;Database=mydatabase;Uid=root;Pwd=123;";
        optionsBuilder.UseMySQL(connectionString);
        return new MySqlContext(optionsBuilder.Options);
    }
}