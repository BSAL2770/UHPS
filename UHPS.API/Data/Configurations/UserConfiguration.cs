using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UHPS.API.Entities;

namespace UHPS.API.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const int AdminRoleId = 1;

    private static readonly DateTime SeedTimestamp = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // DEV ONLY — precomputed BCrypt hash of "Admin@123!" (work factor 11).
    // The seeded admin is for local dev so role-gated routes can be exercised.
    // Production deployments must NOT use this hash; rotate by inserting a new
    // admin via env-driven seed or psql, then deleting this row.
    private const string DevAdminPasswordHash =
        "$2a$11$3uIl8JsTv2Lsecj1l7XbUeJJr.3YwfpOviL0YS4vw34NaGP3hfZN2";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        BaseEntityConfiguration.ConfigureBase(builder);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new
        {
            Id = 1,
            Email = "admin@uhps.local",
            PasswordHash = DevAdminPasswordHash,
            RoleId = AdminRoleId,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
            Deleted = false
        });
    }
}
