using UHPS.API.Common;
using UHPS.API.Dtos.Packages;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public interface IPackageService
{
    Task<PagedResult<PackageResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct);
    Task<PagedResult<PackageResponse>> GetCurrentCustomerPackagesAsync(PagingQuery paging, CancellationToken ct);
    Task<PackageResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PackageResponse> CreateAsync(PackageCreateRequest request, CancellationToken ct);
    Task<PackageResponse?> UpdateAsync(int id, PackageUpdateRequest request, CancellationToken ct);
    Task<PackageResponse?> UpdateStatusAsync(int id, PackageStatus newStatus, CancellationToken ct);
}
