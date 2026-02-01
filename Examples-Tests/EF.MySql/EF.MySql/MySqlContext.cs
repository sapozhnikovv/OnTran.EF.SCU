using Microsoft.EntityFrameworkCore;
namespace EF.MySql;

public class MySqlContext: DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public MySqlContext(DbContextOptions<MySqlContext> options): base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MySqlContext).Assembly);
    }
}
