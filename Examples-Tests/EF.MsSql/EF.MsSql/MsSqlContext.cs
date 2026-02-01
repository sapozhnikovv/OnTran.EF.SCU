using Microsoft.EntityFrameworkCore;

namespace EF.MsSql;

public class MsSqlContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public MsSqlContext(DbContextOptions<MsSqlContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MsSqlContext).Assembly);
    }
}
