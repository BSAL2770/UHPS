using UHPS.API.Common;
using UHPS.API.Dtos.Shipments;

namespace UHPS.API.Services;

public interface IShipmentService
{
    Task<PagedResult<ShipmentResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct);
    Task<ShipmentResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ShipmentResponse> CreateAsync(ShipmentCreateRequest request, CancellationToken ct);
    Task<ShipmentResponse?> UpdateAsync(int id, ShipmentUpdateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
