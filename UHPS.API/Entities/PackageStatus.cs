namespace UHPS.API.Entities;

// Logical progression: Created -> InTransit -> OutForDelivery -> Delivered, with Returned/Lost as terminal off-paths.
// Numeric values are append-only (OutForDelivery=5) to avoid data migrations on existing Packages.Status.
// API surface uses string names via JsonStringEnumConverter, so callers never see the integer ordering.
public enum PackageStatus
{
    Created = 0,
    InTransit = 1,
    Delivered = 2,
    Returned = 3,
    Lost = 4,
    OutForDelivery = 5
}
