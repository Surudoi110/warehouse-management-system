using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Core.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public MovementType MovementType { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }   // always positive — direction comes from MovementType

    [StringLength(100)]
    [Display(Name = "Reference No.")]
    public string? ReferenceNo { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(450)]
    public string? PerformedById { get; set; }   // FK to AspNetUsers.Id

    [Display(Name = "Performed At")]
    public DateTime PerformedAt { get; set; }
}