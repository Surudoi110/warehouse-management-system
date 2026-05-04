using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Core.Services;

public class StockMovementRequest
{
    [Display(Name = "Product")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a product.")]
    public int ProductId { get; set; }

    [Display(Name = "Location")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a location.")]
    public int LocationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [StringLength(100)]
    [Display(Name = "Reference No. (PO #, shipment #, etc.)")]
    public string? ReferenceNo { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class StockOperationResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public int? MovementId { get; }

    private StockOperationResult(bool success, string? error, int? movementId)
    {
        Success = success;
        ErrorMessage = error;
        MovementId = movementId;
    }

    public static StockOperationResult Ok(int movementId) => new(true, null, movementId);
    public static StockOperationResult Fail(string error) => new(false, error, null);
}