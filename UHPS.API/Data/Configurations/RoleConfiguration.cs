using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UHPS.API.Entities;

namespace UHPS.API.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly DateTime SeedTimestamp = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        BaseEntityConfiguration.ConfigureBase(builder);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = "Admin", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp, Deleted = false },
            new { Id = 2, Name = "Supervisor", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp, Deleted = false },
            new { Id = 3, Name = "Employee", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp, Deleted = false },
            new { Id = 4, Name = "Customer", CreatedAt = SeedTimestamp, UpdatedAt = SeedTimestamp, Deleted = false }
        );
    }
}
