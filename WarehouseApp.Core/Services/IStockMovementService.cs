namespace WarehouseApp.Core.Services;

public interface IStockMovementService
{
    Task<StockOperationResult> ReceiveAsync(
        StockMovementRequest request, string performedByUserId, CancellationToken ct = default);

    Task<StockOperationResult> ShipAsync(
        StockMovementRequest request, string performedByUserId, CancellationToken ct = default);
}