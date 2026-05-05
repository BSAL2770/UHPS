using UHPS.API.Dtos.Tracking;

namespace UHPS.API.Services;

public interface ITrackingService
{
    Task<PackageTrackingResponse?> GetPackageHistoryAsync(int packageId, CancellationToken ct);
    Task<PublicTrackingResponse?> GetPublicTrackingAsync(int packageId, CancellationToken ct);
    Task<TrackingEventResponse?> ScanAsync(int packageId, ScanRequest request, CancellationToken ct);
}
