using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UHPS.API.Entities;

namespace UHPS.API.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        BaseEntityConfiguration.ConfigureBase(builder);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.MaxLength).HasPrecision(8, 2);
        builder.Property(s => s.MaxWidth).HasPrecision(8, 2);
        builder.Property(s => s.MaxHeight).HasPrecision(8, 2);
        builder.Property(s => s.GroundCost).HasPrecision(18, 2);
        builder.Property(s => s.ExpressCost).HasPrecision(18, 2);
    }
}
