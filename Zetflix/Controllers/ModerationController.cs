using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Services;

namespace Zetflix.Controllers;

[Authorize(Policy = "ModeratorOrAdmin")]
public class ModerationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ModerationController(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var reviews = await _context.Reviews
            .Include(review => review.User)
            .Include(review => review.Movie)
            .Include(review => review.Show)
            .OrderByDescending(review => review.Id)
            .ToListAsync();

        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(currentReview => currentReview.Id == id);

        if (review == null)
        {
            return NotFound();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("moderation.review.delete", User.Identity?.Name, $"Review {id} was deleted by moderator.");

        return RedirectToAction(nameof(Index));
    }
}
