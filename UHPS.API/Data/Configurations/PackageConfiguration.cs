using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UHPS.API.Entities;

namespace UHPS.API.Data.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("Packages");
        BaseEntityConfiguration.ConfigureBase(builder);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Weight).HasPrecision(8, 2);
        builder.Property(p => p.Width).HasPrecision(8, 2);
        builder.Property(p => p.Height).HasPrecision(8, 2);
        builder.Property(p => p.Depth).HasPrecision(8, 2);
        builder.Property(p => p.ShipCost).HasPrecision(18, 2);

        builder.HasOne(p => p.Sender)
            .WithMany(c => c.SentPackages)
            .HasForeignKey(p => p.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Receiver)
            .WithMany(c => c.ReceivedPackages)
            .HasForeignKey(p => p.ReceiverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Address)
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Shipment)
            .WithMany(s => s.Packages)
            .HasForeignKey(p => p.ShipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
