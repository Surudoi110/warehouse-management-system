using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Core.Entities;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = "";

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, 1_000_000)]
    [Display(Name = "Unit Price")]
    [DataType(DataType.Currency)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Reorder Level")]
    public int ReorderLevel { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<InventoryItem> InventoryItems { get; set; } = new();

    public List<StockMovement> Movements { get; set; } = new();
}