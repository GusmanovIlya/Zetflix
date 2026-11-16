using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Services;

namespace Zetflix.Controllers;

public class SubscriptionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public SubscriptionsController(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var subscriptions = await _context.Subscriptions
            .OrderBy(subscription => subscription.Price)
            .ToListAsync();

        return View(subscriptions);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Choose(int id)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var subscriptionExists = await _context.Subscriptions.AnyAsync(subscription => subscription.Id == id);

        if (!subscriptionExists)
        {
            return NotFound();
        }

        var user = await _context.Users.FirstOrDefaultAsync(currentUser => currentUser.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        user.SubscriptionId = id;
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("subscription.choose", User.Identity?.Name, $"Subscription {id} selected.");

        return RedirectToAction("Index", "Profile");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _context.Users.FirstOrDefaultAsync(currentUser => currentUser.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        user.SubscriptionId = null;
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("subscription.cancel", User.Identity?.Name, "Subscription cancelled.");

        return RedirectToAction("Index", "Profile");
    }
}
