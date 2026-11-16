using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;
using Zetflix.Services;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ReviewsController(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Include(r => r.Show)
            .OrderByDescending(review => review.Id)
            .ToListAsync();

        return View(reviews);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var model = await BuildReviewFormModel(new ReviewCreateViewModel());
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel model)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        await ValidateReviewTarget(model);

        if (!ModelState.IsValid)
        {
            model = await BuildReviewFormModel(model);
            return View(model);
        }

        var review = new Review
        {
            Comment = model.Comment,
            Rating = model.Rating,
            UserId = userId.Value,
            MovieId = model.MovieId,
            ShowId = model.ShowId
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("review.create", User.Identity?.Name, $"Review {review.Id} was created.");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review == null)
        {
            return NotFound();
        }

        var model = new ReviewCreateViewModel
        {
            Rating = review.Rating,
            Comment = review.Comment,
            MovieId = review.MovieId,
            ShowId = review.ShowId
        };

        model = await BuildReviewFormModel(model);
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReviewCreateViewModel model)
    {
        var userId = GetCurrentUserId();
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review == null)
        {
            return NotFound();
        }

        await ValidateReviewTarget(model);

        if (!ModelState.IsValid)
        {
            model = await BuildReviewFormModel(model);
            return View(model);
        }

        review.Rating = model.Rating;
        review.Comment = model.Comment;
        review.MovieId = model.MovieId;
        review.ShowId = model.ShowId;

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("review.update", User.Identity?.Name, $"Review {review.Id} was updated.");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Include(r => r.Show)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review == null)
        {
            return NotFound();
        }

        return View(review);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetCurrentUserId();
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review == null)
        {
            return NotFound();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("review.delete", User.Identity?.Name, $"Review {review.Id} was deleted.");

        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var id) ? id : null;
    }

    private async Task ValidateReviewTarget(ReviewCreateViewModel model)
    {
        if (model.MovieId == null && model.ShowId == null)
        {
            ModelState.AddModelError(string.Empty, "Нужно выбрать фильм или сериал.");
            return;
        }

        if (model.MovieId != null && model.ShowId != null)
        {
            ModelState.AddModelError(string.Empty, "Можно выбрать только фильм или только сериал.");
            return;
        }

        if (model.MovieId != null && !await _context.Movies.AnyAsync(movie => movie.Id == model.MovieId.Value))
        {
            ModelState.AddModelError(nameof(model.MovieId), "Выбранный фильм не найден.");
        }

        if (model.ShowId != null && !await _context.Shows.AnyAsync(show => show.Id == model.ShowId.Value))
        {
            ModelState.AddModelError(nameof(model.ShowId), "Выбранный сериал не найден.");
        }
    }

    private async Task<ReviewCreateViewModel> BuildReviewFormModel(ReviewCreateViewModel model)
    {
        model.Movies = await _context.Movies
            .OrderBy(m => m.Title)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Title
            })
            .ToListAsync();

        model.Shows = await _context.Shows
            .OrderBy(s => s.Title)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Title
            })
            .ToListAsync();

        return model;
    }
}
