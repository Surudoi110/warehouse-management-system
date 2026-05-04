using Microsoft.EntityFrameworkCore;
using WarehouseApp.Core.Entities;
using WarehouseApp.Core.Services;
using WarehouseApp.Infrastructure.Data;

namespace WarehouseApp.Infrastructure.Services;

public class StockMovementService : IStockMovementService
{
    private readonly ApplicationDbContext _db;

    public StockMovementService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<StockOperationResult> ReceiveAsync(
        StockMovementRequest request, string performedByUserId, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
            return StockOperationResult.Fail("Quantity must be positive.");

        if (!await _db.Products.AnyAsync(p => p.Id == request.ProductId, ct))
            return StockOperationResult.Fail("Product not found.");

        if (!await _db.Locations.AnyAsync(l => l.Id == request.LocationId, ct))
            return StockOperationResult.Fail("Location not found.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            LocationId = request.LocationId,
            MovementType = MovementType.Receive,
            Quantity = request.Quantity,
            ReferenceNo = request.ReferenceNo,
            Notes = request.Notes,
            PerformedById = performedByUserId,
            PerformedAt = DateTime.UtcNow
        };
        _db.StockMovements.Add(movement);

        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(
            i => i.ProductId == request.ProductId && i.LocationId == request.LocationId, ct);

        if (inventory == null)
        {
            inventory = new InventoryItem
            {
                ProductId = request.ProductId,
                LocationId = request.LocationId,
                Quantity = 0
            };
            _db.InventoryItems.Add(inventory);
        }
        inventory.Quantity += request.Quantity;

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return StockOperationResult.Ok(movement.Id);
    }

    public async Task<StockOperationResult> ShipAsync(
        StockMovementRequest request, string performedByUserId, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
            return StockOperationResult.Fail("Quantity must be positive.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var inventory = await _db.InventoryItems.FirstOrDefaultAsync(
            i => i.ProductId == request.ProductId && i.LocationId == request.LocationId, ct);

        var available = inventory?.Quantity ?? 0;
        if (available < request.Quantity)
            return StockOperationResult.Fail(
                $"Insufficient stock at this location. Available: {available}, requested: {request.Quantity}.");

        inventory!.Quantity -= request.Quantity;

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            LocationId = request.LocationId,
            MovementType = MovementType.Ship,
            Quantity = request.Quantity,
            ReferenceNo = request.ReferenceNo,
            Notes = request.Notes,
            PerformedById = performedByUserId,
            PerformedAt = DateTime.UtcNow
        };
        _db.StockMovements.Add(movement);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return StockOperationResult.Ok(movement.Id);
    }
}