using UHPS.API.Common;
using UHPS.API.Dtos.Employees;

namespace UHPS.API.Services;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct);
    Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<EmployeeResponse?> GetCurrentAsync(CancellationToken ct);
    Task<EmployeeResponse> CreateAsync(EmployeeCreateRequest request, CancellationToken ct);
    Task<EmployeeResponse?> UpdateAsync(int id, EmployeeUpdateRequest request, CancellationToken ct);
    Task<EmployeeResponse?> UpdateCurrentAsync(EmployeeSelfUpdateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
