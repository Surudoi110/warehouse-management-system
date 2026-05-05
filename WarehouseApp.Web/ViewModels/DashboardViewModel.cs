using WarehouseApp.Core.Entities;

namespace WarehouseApp.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalLocations { get; set; }
    public int TotalUnits { get; set; }
    public int LowStockCount { get; set; }

    public List<LowStockRow> LowStockItems { get; set; } = new();
    public List<RecentMovementRow> RecentMovements { get; set; } = new();
    public List<DailyMovementPoint> Last7Days { get; set; } = new();
}

public class LowStockRow
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
}

public class RecentMovementRow
{
    public DateTime When { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; } = "";
    public string LocationCode { get; set; } = "";
}

public class DailyMovementPoint
{
    public DateTime Date { get; set; }
    public int Received { get; set; }
    public int Shipped { get; set; }
}