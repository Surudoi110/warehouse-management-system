using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WarehouseApp.Core.Entities;
using WarehouseApp.Core.Services;
using WarehouseApp.Infrastructure.Data;
using WarehouseApp.Infrastructure.Services;
using Xunit;

namespace WarehouseApp.Tests;

public class StockMovementServiceTests
{
    // Each test gets its own isolated in-memory database via a unique name (Guid)
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // Our service uses BeginTransactionAsync — InMemory provider doesn't really
            // support transactions but it warns. We suppress the warning so tests don't fail.
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(int productId, int locationId)> SeedAsync(ApplicationDbContext db)
    {
        var product = new Product { Sku = "TEST-001", Name = "Test Product", UnitPrice = 10m };
        var location = new Location { Code = "A-01", Description = "Test location" };
        db.Products.Add(product);
        db.Locations.Add(location);
        await db.SaveChangesAsync();
        return (product.Id, location.Id);
    }

    // ---------- ReceiveAsync ----------

    [Fact]
    public async Task ReceiveAsync_CreatesNewInventoryItem_WhenNoneExists()
    {
        using var db = CreateDb();
        var (productId, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        var result = await service.ReceiveAsync(new StockMovementRequest
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = 100,
            ReferenceNo = "PO-001"
        }, performedByUserId: "test-user");

        Assert.True(result.Success);
        var inv = await db.InventoryItems.SingleAsync();
        Assert.Equal(100, inv.Quantity);
        Assert.Equal(1, await db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task ReceiveAsync_AddsToExistingInventoryItem()
    {
        using var db = CreateDb();
        var (productId, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        await service.ReceiveAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 50 }, "test-user");
        await service.ReceiveAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 30 }, "test-user");

        var inv = await db.InventoryItems.SingleAsync();
        Assert.Equal(80, inv.Quantity);
        Assert.Equal(2, await db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task ReceiveAsync_RejectsZeroQuantity()
    {
        using var db = CreateDb();
        var (productId, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        var result = await service.ReceiveAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 0 }, "test-user");

        Assert.False(result.Success);
        Assert.Empty(await db.InventoryItems.ToListAsync());
        Assert.Empty(await db.StockMovements.ToListAsync());
    }

    [Fact]
    public async Task ReceiveAsync_RejectsUnknownProduct()
    {
        using var db = CreateDb();
        var (_, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        var result = await service.ReceiveAsync(new StockMovementRequest
            { ProductId = 9999, LocationId = locationId, Quantity = 10 }, "test-user");

        Assert.False(result.Success);
        Assert.Contains("Product not found", result.ErrorMessage);
    }

    // ---------- ShipAsync ----------

    [Fact]
    public async Task ShipAsync_SubtractsFromInventory()
    {
        using var db = CreateDb();
        var (productId, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        await service.ReceiveAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 100 }, "test-user");

        var result = await service.ShipAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 30 }, "test-user");

        Assert.True(result.Success);
        var inv = await db.InventoryItems.SingleAsync();
        Assert.Equal(70, inv.Quantity);
    }

    [Fact]
    public async Task ShipAsync_RejectsOversell()
    {
        using var db = CreateDb();
        var (productId, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        await service.ReceiveAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 50 }, "test-user");

        var result = await service.ShipAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 100 }, "test-user");

        Assert.False(result.Success);
        Assert.Contains("Insufficient stock", result.ErrorMessage);

        // Inventory should remain unchanged at 50
        var inv = await db.InventoryItems.SingleAsync();
        Assert.Equal(50, inv.Quantity);

        // Only the receive movement should have been recorded
        Assert.Equal(1, await db.StockMovements.CountAsync());
    }

    [Fact]
    public async Task ShipAsync_RejectsWhenNoInventoryExists()
    {
        using var db = CreateDb();
        var (productId, locationId) = await SeedAsync(db);
        var service = new StockMovementService(db);

        var result = await service.ShipAsync(new StockMovementRequest
            { ProductId = productId, LocationId = locationId, Quantity = 1 }, "test-user");

        Assert.False(result.Success);
        Assert.Contains("Insufficient stock", result.ErrorMessage);
    }
}