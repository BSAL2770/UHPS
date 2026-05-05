using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UHPS.API.Entities;

namespace UHPS.API.Data.Configurations;

public class TrackingRecordConfiguration : IEntityTypeConfiguration<TrackingRecord>
{
    public void Configure(EntityTypeBuilder<TrackingRecord> builder)
    {
        builder.ToTable("TrackingRecords");
        BaseEntityConfiguration.ConfigureBase(builder);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.HasOne(t => t.Employee)
            .WithMany(e => e.TrackingRecords)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Package)
            .WithMany(p => p.TrackingRecords)
            .HasForeignKey(t => t.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Store)
            .WithMany(s => s.TrackingRecords)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
