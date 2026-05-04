using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Infrastructure.Data;

namespace WarehouseApp.Web.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _db;

    public InventoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.InventoryItems
            .Include(i => i.Product)
            .Include(i => i.Location)
            .OrderBy(i => i.Product!.Name)
            .ThenBy(i => i.Location!.Code)
            .ToListAsync();

        return View(items);
    }
}