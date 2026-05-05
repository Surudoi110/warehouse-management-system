using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Infrastructure.Data;

namespace WarehouseApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var rows = new List<UserRoleViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            rows.Add(new UserRoleViewModel
            {
                UserId = u.Id,
                Email = u.Email ?? "",
                CurrentRole = roles.FirstOrDefault() ?? "(none)"
            });
        }
        ViewBag.AvailableRoles = SeedData.Roles;
        return View(rows);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Remove from all roles, then add the new one (single-role per user model)
        var existing = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, existing);

        if (!string.IsNullOrEmpty(role))
            await _userManager.AddToRoleAsync(user, role);

        TempData["SuccessMessage"] = $"Updated {user.Email} to role {role}.";
        return RedirectToAction(nameof(Index));
    }
}

public class UserRoleViewModel
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string CurrentRole { get; set; } = "";
}