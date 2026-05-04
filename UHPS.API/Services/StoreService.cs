using Microsoft.EntityFrameworkCore;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Common;
using UHPS.API.Dtos.Stores;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public class StoreService : IStoreService
{
    private readonly AppDbContext _db;

    public StoreService(AppDbContext db)
    {
        _db = db;
    }

    public Task<PagedResult<StoreResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct) =>
        _db.Stores
            .Include(s => s.Address)
            .Include(s => s.Supervisor)
            .OrderBy(s => s.Id)
            .ToPagedResultAsync(paging, MapToResponse, ct);

    public async Task<StoreResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var store = await _db.Stores
            .Include(s => s.Address)
            .Include(s => s.Supervisor)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return store is null ? null : MapToResponse(store);
    }

    public async Task<StoreResponse> CreateAsync(StoreCreateRequest request, CancellationToken ct)
    {
        await ValidateSupervisorAsync(request.SupervisorId, ct);

        var store = new Store
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            SupervisorId = request.SupervisorId,
            Address = request.Address is null ? null : MapAddress(request.Address)
        };

        _db.Stores.Add(store);
        await _db.SaveChangesAsync(ct);

        if (store.SupervisorId.HasValue)
            await _db.Entry(store).Reference(s => s.Supervisor).LoadAsync(ct);

        return MapToResponse(store);
    }

    public async Task<StoreResponse?> UpdateAsync(int id, StoreUpdateRequest request, CancellationToken ct)
    {
        var store = await _db.Stores
            .Include(s => s.Address)
            .Include(s => s.Supervisor)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (store is null) return null;

        await ValidateSupervisorAsync(request.SupervisorId, ct);

        store.Name = request.Name;
        store.PhoneNumber = request.PhoneNumber;
        store.SupervisorId = request.SupervisorId;

        if (request.Address is not null)
        {
            if (store.Address is not null)
            {
                store.Address.StreetAddress = request.Address.StreetAddress;
                store.Address.City = request.Address.City;
                store.Address.State = request.Address.State;
                store.Address.Zipcode = request.Address.Zipcode;
            }
            else
            {
                store.Address = MapAddress(request.Address);
            }
        }
        else
        {
            store.AddressId = null;
        }

        await _db.SaveChangesAsync(ct);

        await _db.Entry(store).Reference(s => s.Supervisor).LoadAsync(ct);

        return MapToResponse(store);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (store is null) return false;

        store.Deleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task ValidateSupervisorAsync(int? supervisorId, CancellationToken ct)
    {
        if (!supervisorId.HasValue) return;

        var exists = await _db.Employees.AnyAsync(e => e.Id == supervisorId.Value, ct);
        if (!exists)
            throw new ValidationException($"Supervisor with id {supervisorId} does not exist.");
    }

    private static Address MapAddress(AddressRequest req) => new()
    {
        StreetAddress = req.StreetAddress,
        City = req.City,
        State = req.State,
        Zipcode = req.Zipcode
    };

    private static StoreResponse MapToResponse(Store store) => new()
    {
        Id = store.Id,
        Name = store.Name,
        PhoneNumber = store.PhoneNumber,
        Address = store.Address is null ? null : new AddressResponse
        {
            Id = store.Address.Id,
            StreetAddress = store.Address.StreetAddress,
            City = store.Address.City,
            State = store.Address.State,
            Zipcode = store.Address.Zipcode
        },
        SupervisorId = store.SupervisorId,
        SupervisorName = store.Supervisor?.Name,
        CreatedAt = store.CreatedAt,
        UpdatedAt = store.UpdatedAt
    };
}
