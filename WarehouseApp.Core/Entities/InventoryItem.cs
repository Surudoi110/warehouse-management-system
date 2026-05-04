using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Core.Entities;

public class InventoryItem
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}