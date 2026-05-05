using Microsoft.EntityFrameworkCore;
using UHPS.API.Auth;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Tracking;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public class TrackingService : ITrackingService
{
    private const int PublicEventCap = 5;

    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public TrackingService(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PackageTrackingResponse?> GetPackageHistoryAsync(int packageId, CancellationToken ct)
    {
        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == packageId, ct);
        if (package is null) return null;

        if (!_currentUser.IsInRole(Roles.Admin, Roles.Supervisor, Roles.Employee))
        {
            // Customer must be sender or receiver of this package
            var customerId = await GetCurrentCustomerIdAsync(ct);
            if (customerId is null
                || (package.SenderId != customerId && package.ReceiverId != customerId))
            {
                throw new ForbiddenAccessException();
            }
        }

        var events = await _db.TrackingRecords
            .Include(t => t.Store)
            .Include(t => t.Employee)
            .Where(t => t.PackageId == packageId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        return new PackageTrackingResponse
        {
            PackageId = package.Id,
            CurrentStatus = package.Status,
            Events = events.Select(MapToFullEvent).ToList()
        };
    }

    public async Task<PublicTrackingResponse?> GetPublicTrackingAsync(int packageId, CancellationToken ct)
    {
        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == packageId, ct);
        if (package is null) return null;

        var events = await _db.TrackingRecords
            .Include(t => t.Store)
            .Where(t => t.PackageId == packageId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(PublicEventCap)
            .ToListAsync(ct);

        return new PublicTrackingResponse
        {
            PackageId = package.Id,
            CurrentStatus = package.Status,
            RecentEvents = events.Select(MapToPublicEvent).ToList()
        };
    }

    public async Task<TrackingEventResponse?> ScanAsync(int packageId, ScanRequest request, CancellationToken ct)
    {
        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == packageId, ct);
        if (package is null) return null;

        var storeExists = await _db.Stores.AnyAsync(s => s.Id == request.StoreId, ct);
        if (!storeExists)
            throw new ValidationException($"Store {request.StoreId} does not exist.");

        // Same role rule as PATCH /status: Lost is Admin/Supervisor only.
        // Enforce inside the service so the rule applies whether the caller hits PATCH or scan.
        if (request.NewStatus == PackageStatus.Lost
            && !_currentUser.IsInRole(Roles.Admin, Roles.Supervisor))
        {
            throw new ForbiddenAccessException("Only Admin or Supervisor can mark a package as Lost.");
        }

        // Resolve scanner identity from JWT, not body — staff can't fake who did the scan.
        var employeeId = await GetCurrentEmployeeIdAsync(ct);

        // Status snapshot on the tracking record is the status AFTER this scan
        // (i.e. NewStatus if provided, otherwise the package's current status unchanged).
        var snapshotStatus = request.NewStatus ?? package.Status;

        var record = new TrackingRecord
        {
            PackageId = packageId,
            StoreId = request.StoreId,
            EmployeeId = employeeId,
            Status = snapshotStatus,
            Notes = request.Notes
        };

        _db.TrackingRecords.Add(record);

        if (request.NewStatus.HasValue && request.NewStatus.Value != package.Status)
        {
            package.Status = request.NewStatus.Value;
        }

        // Both the insert and the package update flush in one SaveChanges = one transaction.
        // If either fails, neither persists.
        await _db.SaveChangesAsync(ct);

        var fresh = await _db.TrackingRecords
            .Include(t => t.Store)
            .Include(t => t.Employee)
            .FirstAsync(t => t.Id == record.Id, ct);

        return MapToFullEvent(fresh);
    }

    // ---- helpers ----

    private async Task<int?> GetCurrentCustomerIdAsync(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;
        return await _db.Customers
            .Where(c => c.UserId == _currentUser.UserId.Value)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<int?> GetCurrentEmployeeIdAsync(CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue) return null;
        return await _db.Employees
            .Where(e => e.UserId == _currentUser.UserId.Value)
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync(ct);
    }

    private static TrackingEventResponse MapToFullEvent(TrackingRecord t) => new()
    {
        Id = t.Id,
        ScannedAt = t.CreatedAt,
        Status = t.Status,
        StoreId = t.StoreId,
        StoreName = t.Store.Name,
        EmployeeId = t.EmployeeId,
        EmployeeName = t.Employee?.Name,
        Notes = t.Notes
    };

    private static PublicTrackingEventResponse MapToPublicEvent(TrackingRecord t) => new()
    {
        ScannedAt = t.CreatedAt,
        Status = t.Status,
        StoreName = t.Store.Name
    };
}
