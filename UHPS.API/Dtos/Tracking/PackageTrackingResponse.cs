using UHPS.API.Entities;

namespace UHPS.API.Dtos.Tracking;

public class PackageTrackingResponse
{
    public required int PackageId { get; set; }
    public required PackageStatus CurrentStatus { get; set; }
    public required IReadOnlyList<TrackingEventResponse> Events { get; set; }
}
