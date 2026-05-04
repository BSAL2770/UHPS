using UHPS.API.Common;
using UHPS.API.Dtos.Stores;

namespace UHPS.API.Services;

public interface IStoreService
{
    Task<PagedResult<StoreResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct);
    Task<StoreResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<StoreResponse> CreateAsync(StoreCreateRequest request, CancellationToken ct);
    Task<StoreResponse?> UpdateAsync(int id, StoreUpdateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
