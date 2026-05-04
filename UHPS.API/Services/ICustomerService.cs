using UHPS.API.Common;
using UHPS.API.Dtos.Customers;

namespace UHPS.API.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct);
    Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<CustomerResponse?> GetCurrentAsync(CancellationToken ct);
    Task<CustomerResponse?> UpdateAsync(int id, CustomerUpdateRequest request, CancellationToken ct);
    Task<CustomerResponse?> UpdateCurrentAsync(CustomerUpdateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
