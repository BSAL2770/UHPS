using Microsoft.EntityFrameworkCore;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Common;
using UHPS.API.Dtos.Packages;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public class PackageService : IPackageService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public PackageService(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<PagedResult<PackageResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct) =>
        BasePackageQuery()
            .OrderBy(p => p.Id)
            .ToPagedResultAsync(paging, MapToResponse, ct);

    public async Task<PagedResult<PackageResponse>> GetCurrentCustomerPackagesAsync(
        PagingQuery paging,
        CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerIdAsync(ct);
        if (customerId is null)
        {
            return new PagedResult<PackageResponse>
            {
                Items = Array.Empty<PackageResponse>(),
                Page = 1,
                PageSize = paging.PageSize,
                TotalCount = 0
            };
        }

        return await BasePackageQuery()
            .Where(p => p.SenderId == customerId.Value || p.ReceiverId == customerId.Value)
            .OrderByDescending(p => p.CreatedAt)
            .ToPagedResultAsync(paging, MapToResponse, ct);
    }

    public async Task<PackageResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var package = await BasePackageQuery().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (package is null) return null;

        // Customer can only see packages where they are sender or receiver
        if (_currentUser.IsInRole(Roles.Admin, Roles.Supervisor, Roles.Employee))
            return MapToResponse(package);

        var customerId = await GetCurrentCustomerIdAsync(ct);
        if (customerId is null
            || (package.SenderId != customerId && package.ReceiverId != customerId))
        {
            throw new ForbiddenAccessException();
        }

        return MapToResponse(package);
    }

    public async Task<PackageResponse> CreateAsync(PackageCreateRequest request, CancellationToken ct)
    {
        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == request.ShipmentId, ct)
            ?? throw new ValidationException($"Shipment tier {request.ShipmentId} does not exist.");

        ValidateDimensionsAgainstTier(request, shipment);

        int? senderId;
        if (_currentUser.IsInRole(Roles.Admin, Roles.Supervisor))
        {
            // Staff: SenderId must be supplied and valid (or null for an unattributed package)
            if (request.SenderId.HasValue)
            {
                var senderExists = await _db.Customers.AnyAsync(c => c.Id == request.SenderId.Value, ct);
                if (!senderExists)
                    throw new ValidationException($"Sender customer {request.SenderId} does not exist.");
            }
            senderId = request.SenderId;
        }
        else
        {
            // Customer: SenderId is forced to their own customer id (request value ignored)
            senderId = await GetCurrentCustomerIdAsync(ct)
                ?? throw new ValidationException("No customer profile is linked to this user.");
        }

        if (request.ReceiverId.HasValue)
        {
            var receiverExists = await _db.Customers.AnyAsync(c => c.Id == request.ReceiverId.Value, ct);
            if (!receiverExists)
                throw new ValidationException($"Receiver customer {request.ReceiverId} does not exist.");
        }

        var package = new Package
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Description = request.Description,
            Status = PackageStatus.Created,
            Weight = request.Weight,
            Width = request.Width,
            Height = request.Height,
            Depth = request.Depth,
            Express = request.Express,
            ShipmentId = request.ShipmentId,
            ShipCost = request.Express ? shipment.ExpressCost : shipment.GroundCost,
            Address = MapAddress(request.Address)
        };

        _db.Packages.Add(package);
        await _db.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(package.Id, ct);
    }

    public async Task<PackageResponse?> UpdateAsync(int id, PackageUpdateRequest request, CancellationToken ct)
    {
        var package = await _db.Packages
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (package is null) return null;

        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == request.ShipmentId, ct)
            ?? throw new ValidationException($"Shipment tier {request.ShipmentId} does not exist.");

        ValidateDimensionsAgainstTier(request, shipment);

        if (request.ReceiverId.HasValue)
        {
            var receiverExists = await _db.Customers.AnyAsync(c => c.Id == request.ReceiverId.Value, ct);
            if (!receiverExists)
                throw new ValidationException($"Receiver customer {request.ReceiverId} does not exist.");
        }

        package.ReceiverId = request.ReceiverId;
        package.Description = request.Description;
        package.Weight = request.Weight;
        package.Width = request.Width;
        package.Height = request.Height;
        package.Depth = request.Depth;
        package.Express = request.Express;
        package.ShipmentId = request.ShipmentId;
        package.ShipCost = request.Express ? shipment.ExpressCost : shipment.GroundCost;

        package.Address.StreetAddress = request.Address.StreetAddress;
        package.Address.City = request.Address.City;
        package.Address.State = request.Address.State;
        package.Address.Zipcode = request.Address.Zipcode;

        await _db.SaveChangesAsync(ct);
        return await ReloadAndMapAsync(package.Id, ct);
    }

    public async Task<PackageResponse?> UpdateStatusAsync(int id, PackageStatus newStatus, CancellationToken ct)
    {
        if (newStatus == PackageStatus.Lost && !_currentUser.IsInRole(Roles.Admin, Roles.Supervisor))
            throw new ForbiddenAccessException("Only Admin or Supervisor can mark a package as Lost.");

        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (package is null) return null;

        package.Status = newStatus;
        await _db.SaveChangesAsync(ct);

        return await ReloadAndMapAsync(package.Id, ct);
    }

    // ---- helpers ----

    private IQueryable<Package> BasePackageQuery() =>
        _db.Packages
            .Include(p => p.Address)
            .Include(p => p.Sender)
            .Include(p => p.Receiver)
            .Include(p => p.Shipment);

    private async Task<PackageResponse> ReloadAndMapAsync(int id, CancellationToken ct)
    {
        var fresh = await BasePackageQuery().FirstAsync(p => p.Id == id, ct);
        return MapToResponse(fresh);
    }

    private async Task<int?> GetCurrentCustomerIdAsync(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;
        return await _db.Customers
            .Where(c => c.UserId == _currentUser.UserId.Value)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static void ValidateDimensionsAgainstTier(PackageCreateRequest req, Shipment shipment) =>
        ValidateDimensions(req.Width, req.Height, req.Depth, shipment);

    private static void ValidateDimensionsAgainstTier(PackageUpdateRequest req, Shipment shipment) =>
        ValidateDimensions(req.Width, req.Height, req.Depth, shipment);

    private static void ValidateDimensions(decimal width, decimal height, decimal depth, Shipment shipment)
    {
        // Compare longest provided dimension to MaxLength, then the remaining two to MaxWidth/MaxHeight.
        // This matches "any orientation that fits the tier's bounding box."
        var dims = new[] { width, height, depth };
        Array.Sort(dims);
        // dims[2] = longest, dims[1] = middle, dims[0] = shortest

        if (dims[2] > shipment.MaxLength
            || dims[1] > shipment.MaxWidth
            || dims[0] > shipment.MaxHeight)
        {
            throw new ValidationException(
                $"Package dimensions ({width} x {height} x {depth}) exceed tier '{shipment.Description}' " +
                $"limits ({shipment.MaxLength} x {shipment.MaxWidth} x {shipment.MaxHeight}).");
        }
    }

    private static Address MapAddress(AddressRequest req) => new()
    {
        StreetAddress = req.StreetAddress,
        City = req.City,
        State = req.State,
        Zipcode = req.Zipcode
    };

    private static PackageResponse MapToResponse(Package p) => new()
    {
        Id = p.Id,
        SenderId = p.SenderId,
        SenderName = p.Sender?.Name,
        ReceiverId = p.ReceiverId,
        ReceiverName = p.Receiver?.Name,
        Address = new AddressResponse
        {
            Id = p.Address.Id,
            StreetAddress = p.Address.StreetAddress,
            City = p.Address.City,
            State = p.Address.State,
            Zipcode = p.Address.Zipcode
        },
        Description = p.Description,
        Status = p.Status,
        Weight = p.Weight,
        Width = p.Width,
        Height = p.Height,
        Depth = p.Depth,
        Express = p.Express,
        ShipmentId = p.ShipmentId,
        ShipmentDescription = p.Shipment?.Description,
        ShipCost = p.ShipCost,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
