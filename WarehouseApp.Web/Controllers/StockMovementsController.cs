using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Core.Services;
using WarehouseApp.Infrastructure.Data;

namespace WarehouseApp.Web.Controllers;

[Authorize]
public class StockMovementsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IStockMovementService _service;
    private readonly UserManager<IdentityUser> _userManager;

    public StockMovementsController(
        ApplicationDbContext db,
        IStockMovementService service,
        UserManager<IdentityUser> userManager)
    {
        _db = db;
        _service = service;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var movements = await _db.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Location)
            .OrderByDescending(m => m.PerformedAt)
            .Take(100)
            .ToListAsync();
        return View(movements);
    }

    public async Task<IActionResult> Receive()
    {
        await PopulateDropdownsAsync();
        return View(new StockMovementRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(StockMovementRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(request);
        }

        var userId = _userManager.GetUserId(User)!;
        var result = await _service.ReceiveAsync(request, userId);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage!);
            await PopulateDropdownsAsync();
            return View(request);
        }

        TempData["SuccessMessage"] = $"Received {request.Quantity} units successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Ship()
    {
        await PopulateDropdownsAsync();
        return View(new StockMovementRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(StockMovementRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(request);
        }

        var userId = _userManager.GetUserId(User)!;
        var result = await _service.ShipAsync(request, userId);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage!);
            await PopulateDropdownsAsync();
            return View(request);
        }

        TempData["SuccessMessage"] = $"Shipped {request.Quantity} units successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Products = new SelectList(
            await _db.Products.OrderBy(p => p.Name).ToListAsync(),
            "Id", "Name");
        ViewBag.Locations = new SelectList(
            await _db.Locations.OrderBy(l => l.Code).ToListAsync(),
            "Id", "Code");
    }
}