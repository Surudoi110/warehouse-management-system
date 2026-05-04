using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Core.Entities;

public class Location
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Location Code")]
    public string Code { get; set; } = "";

    [StringLength(200)]
    public string? Description { get; set; }

    public List<InventoryItem> InventoryItems { get; set; } = new();

    public List<StockMovement> Movements { get; set; } = new();
}