using Microsoft.EntityFrameworkCore;
using UHPS.API.Common;
using UHPS.API.Data;
using UHPS.API.Dtos.Shipments;
using UHPS.API.Entities;

namespace UHPS.API.Services;

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _db;

    public ShipmentService(AppDbContext db)
    {
        _db = db;
    }

    public Task<PagedResult<ShipmentResponse>> GetAllAsync(PagingQuery paging, CancellationToken ct) =>
        _db.Shipments
            .OrderBy(s => s.Id)
            .ToPagedResultAsync(paging, MapToResponse, ct);

    public async Task<ShipmentResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id, ct);
        return shipment is null ? null : MapToResponse(shipment);
    }

    public async Task<ShipmentResponse> CreateAsync(ShipmentCreateRequest request, CancellationToken ct)
    {
        var shipment = new Shipment
        {
            Description = request.Description,
            MaxLength = request.MaxLength,
            MaxWidth = request.MaxWidth,
            MaxHeight = request.MaxHeight,
            GroundCost = request.GroundCost,
            ExpressCost = request.ExpressCost
        };

        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(shipment);
    }

    public async Task<ShipmentResponse?> UpdateAsync(int id, ShipmentUpdateRequest request, CancellationToken ct)
    {
        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shipment is null) return null;

        shipment.Description = request.Description;
        shipment.MaxLength = request.MaxLength;
        shipment.MaxWidth = request.MaxWidth;
        shipment.MaxHeight = request.MaxHeight;
        shipment.GroundCost = request.GroundCost;
        shipment.ExpressCost = request.ExpressCost;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(shipment);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shipment is null) return false;

        shipment.Deleted = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ShipmentResponse MapToResponse(Shipment s) => new()
    {
        Id = s.Id,
        Description = s.Description,
        MaxLength = s.MaxLength,
        MaxWidth = s.MaxWidth,
        MaxHeight = s.MaxHeight,
        GroundCost = s.GroundCost,
        ExpressCost = s.ExpressCost,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
