using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id))
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        var reviews = await _context.Reviews
            .Include(review => review.Movie)
            .Include(review => review.Show)
            .Where(review => review.UserId == id)
            .OrderByDescending(review => review.Id)
            .ToListAsync();

        var model = new ProfileViewModel
        {
            User = user,
            Reviews = reviews
        };

        return View(model);
    }
}
