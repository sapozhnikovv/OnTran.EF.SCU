using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF.Postgres;

public class TestEntityConfiguration : IEntityTypeConfiguration<TestEntity>
{
    public void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        builder.ToTable("TestEntities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();
        builder.Property(e => e.Value)
               .HasMaxLength(500);
    }
}
