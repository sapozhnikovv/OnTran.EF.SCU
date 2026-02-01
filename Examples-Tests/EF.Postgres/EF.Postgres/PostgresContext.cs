using Microsoft.EntityFrameworkCore;

namespace EF.Postgres;

public class PostgresContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresContext).Assembly);
    }
}
