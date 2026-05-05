using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Core.Entities;
using WarehouseApp.Infrastructure.Data;
using WarehouseApp.Web.ViewModels;

namespace WarehouseApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity!.IsAuthenticated)
        {
            return View("Welcome");
        }

        var vm = new DashboardViewModel
        {
            TotalProducts  = await _db.Products.CountAsync(),
            TotalLocations = await _db.Locations.CountAsync(),
            TotalUnits     = await _db.InventoryItems.SumAsync(i => (int?)i.Quantity) ?? 0
        };

        // Low stock: total quantity of a product across all locations <= its reorder level
        var products = await _db.Products.ToListAsync();
        var inventoryByProduct = await _db.InventoryItems
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Total);

        vm.LowStockItems = products
            .Select(p => new LowStockRow
            {
                Sku = p.Sku,
                Name = p.Name,
                CurrentStock = inventoryByProduct.TryGetValue(p.Id, out var qty) ? qty : 0,
                ReorderLevel = p.ReorderLevel
            })
            .Where(r => r.CurrentStock <= r.ReorderLevel)
            .OrderBy(r => r.CurrentStock)
            .Take(10)
            .ToList();
        vm.LowStockCount = vm.LowStockItems.Count;

        // Recent movements
        var recent = await _db.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Location)
            .OrderByDescending(m => m.PerformedAt)
            .Take(10)
            .ToListAsync();

        vm.RecentMovements = recent.Select(m => new RecentMovementRow
        {
            When = m.PerformedAt,
            Type = m.MovementType,
            Quantity = m.Quantity,
            ProductName = m.Product?.Name ?? "",
            LocationCode = m.Location?.Code ?? ""
        }).ToList();

        // Last 7 days for the chart
        var since = DateTime.UtcNow.Date.AddDays(-6);
        var rawMovements = await _db.StockMovements
            .Where(m => m.PerformedAt >= since)
            .Select(m => new { m.PerformedAt, m.MovementType, m.Quantity })
            .ToListAsync();

        vm.Last7Days = Enumerable.Range(0, 7)
            .Select(i => since.AddDays(i))
            .Select(d => new DailyMovementPoint
            {
                Date = d,
                Received = rawMovements
                    .Where(m => m.PerformedAt.Date == d && m.MovementType == MovementType.Receive)
                    .Sum(m => m.Quantity),
                Shipped = rawMovements
                    .Where(m => m.PerformedAt.Date == d && m.MovementType == MovementType.Ship)
                    .Sum(m => m.Quantity)
            })
            .ToList();

        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult Privacy() => View();
}