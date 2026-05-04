using Microsoft.EntityFrameworkCore;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Common;
using UHPS.API.Dtos.Employees;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public EmployeeService(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<PagedResult<EmployeeResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct) =>
        _db.Employees
            .Include(e => e.Address)
            .Include(e => e.User).ThenInclude(u => u!.Role)
            .OrderBy(e => e.Id)
            .ToPagedResultAsync(paging, MapToResponse, ct);

    public async Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var employee = await _db.Employees
            .Include(e => e.Address)
            .Include(e => e.User).ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (employee is null) return null;

        AuthorizationGuards.EnsureOwnerOrInRole(
            _currentUser, employee.UserId,
            Roles.Admin, Roles.Supervisor);

        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse?> GetCurrentAsync(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;

        var employee = await _db.Employees
            .Include(e => e.Address)
            .Include(e => e.User).ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId.Value, ct);

        return employee is null ? null : MapToResponse(employee);
    }

    public async Task<EmployeeResponse> CreateAsync(EmployeeCreateRequest request, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
            throw new ValidationException($"Email '{request.Email}' is already registered.");

        if (await _db.Employees.AnyAsync(e => e.PhoneNumber == request.PhoneNumber, ct))
            throw new ValidationException($"Phone number '{request.PhoneNumber}' is already in use.");

        if (request.StoreId.HasValue)
        {
            var storeExists = await _db.Stores.AnyAsync(s => s.Id == request.StoreId.Value, ct);
            if (!storeExists)
                throw new ValidationException($"Store with id {request.StoreId} does not exist.");
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role, ct);
        if (role is null)
            throw new ValidationException($"Role '{request.Role}' is not a known role.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            RoleId = role.Id
        };

        var employee = new Employee
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            StoreId = request.StoreId,
            User = user,
            Address = MapAddress(request.Address)
        };

        _db.Users.Add(user);
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(user).Reference(u => u.Role).LoadAsync(ct);
        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse?> UpdateAsync(int id, EmployeeUpdateRequest request, CancellationToken ct)
    {
        var employee = await _db.Employees
            .Include(e => e.Address)
            .Include(e => e.User).ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (employee is null) return null;

        if (request.StoreId.HasValue)
        {
            var storeExists = await _db.Stores.AnyAsync(s => s.Id == request.StoreId.Value, ct);
            if (!storeExists)
                throw new ValidationException($"Store with id {request.StoreId} does not exist.");
        }

        if (!string.Equals(employee.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal))
        {
            var phoneTaken = await _db.Employees
                .AnyAsync(e => e.Id != id && e.PhoneNumber == request.PhoneNumber, ct);
            if (phoneTaken)
                throw new ValidationException($"Phone number '{request.PhoneNumber}' is already in use.");
        }

        employee.Name = request.Name;
        employee.PhoneNumber = request.PhoneNumber;
        employee.StoreId = request.StoreId;
        UpdateAddressInPlace(employee.Address, request.Address);

        await _db.SaveChangesAsync(ct);
        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse?> UpdateCurrentAsync(EmployeeSelfUpdateRequest request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;

        var employee = await _db.Employees
            .Include(e => e.Address)
            .Include(e => e.User).ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId.Value, ct);

        if (employee is null) return null;

        if (!string.Equals(employee.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal))
        {
            var phoneTaken = await _db.Employees
                .AnyAsync(e => e.Id != employee.Id && e.PhoneNumber == request.PhoneNumber, ct);
            if (phoneTaken)
                throw new ValidationException($"Phone number '{request.PhoneNumber}' is already in use.");
        }

        employee.Name = request.Name;
        employee.PhoneNumber = request.PhoneNumber;
        UpdateAddressInPlace(employee.Address, request.Address);

        await _db.SaveChangesAsync(ct);
        return MapToResponse(employee);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null) return false;

        employee.Deleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static void UpdateAddressInPlace(Address address, AddressRequest request)
    {
        address.StreetAddress = request.StreetAddress;
        address.City = request.City;
        address.State = request.State;
        address.Zipcode = request.Zipcode;
    }

    private static Address MapAddress(AddressRequest req) => new()
    {
        StreetAddress = req.StreetAddress,
        City = req.City,
        State = req.State,
        Zipcode = req.Zipcode
    };

    private static EmployeeResponse MapToResponse(Employee e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        PhoneNumber = e.PhoneNumber,
        Address = new AddressResponse
        {
            Id = e.Address.Id,
            StreetAddress = e.Address.StreetAddress,
            City = e.Address.City,
            State = e.Address.State,
            Zipcode = e.Address.Zipcode
        },
        StoreId = e.StoreId,
        UserId = e.UserId,
        Email = e.User?.Email,
        Role = e.User?.Role?.Name,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
