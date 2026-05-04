using Microsoft.EntityFrameworkCore;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Common;
using UHPS.API.Dtos.Customers;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public CustomerService(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<PagedResult<CustomerResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct) =>
        _db.Customers
            .Include(c => c.Address)
            .Include(c => c.User)
            .OrderBy(c => c.Id)
            .ToPagedResultAsync(paging, MapToResponse, ct);

    public async Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var customer = await _db.Customers
            .Include(c => c.Address)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (customer is null) return null;

        AuthorizationGuards.EnsureOwnerOrInRole(
            _currentUser, customer.UserId,
            Roles.Admin, Roles.Supervisor, Roles.Employee);

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse?> GetCurrentAsync(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;

        var customer = await _db.Customers
            .Include(c => c.Address)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId.Value, ct);

        return customer is null ? null : MapToResponse(customer);
    }

    public async Task<CustomerResponse?> UpdateAsync(int id, CustomerUpdateRequest request, CancellationToken ct)
    {
        var customer = await _db.Customers
            .Include(c => c.Address)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (customer is null) return null;

        AuthorizationGuards.EnsureOwnerOrInRole(
            _currentUser, customer.UserId,
            Roles.Admin, Roles.Supervisor);

        ApplyUpdate(customer, request);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse?> UpdateCurrentAsync(CustomerUpdateRequest request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;

        var customer = await _db.Customers
            .Include(c => c.Address)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId.Value, ct);

        if (customer is null) return null;

        ApplyUpdate(customer, request);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(customer);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (customer is null) return false;

        customer.Deleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static void ApplyUpdate(Customer customer, CustomerUpdateRequest request)
    {
        customer.Name = request.Name;
        customer.PhoneNumber = request.PhoneNumber;

        if (request.Address is not null)
        {
            if (customer.Address is not null)
            {
                customer.Address.StreetAddress = request.Address.StreetAddress;
                customer.Address.City = request.Address.City;
                customer.Address.State = request.Address.State;
                customer.Address.Zipcode = request.Address.Zipcode;
            }
            else
            {
                customer.Address = new Address
                {
                    StreetAddress = request.Address.StreetAddress,
                    City = request.Address.City,
                    State = request.Address.State,
                    Zipcode = request.Address.Zipcode
                };
            }
        }
        else
        {
            customer.AddressId = null;
        }
    }

    private static CustomerResponse MapToResponse(Customer c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        PhoneNumber = c.PhoneNumber,
        Address = c.Address is null ? null : new AddressResponse
        {
            Id = c.Address.Id,
            StreetAddress = c.Address.StreetAddress,
            City = c.Address.City,
            State = c.Address.State,
            Zipcode = c.Address.Zipcode
        },
        UserId = c.UserId,
        Email = c.User?.Email,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
