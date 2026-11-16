using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/reviews")]
public class AdminReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminReviewDto>>> GetReviews()
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(review => review.User)
            .Include(review => review.Movie)
            .Include(review => review.Show)
            .OrderBy(review => review.Id)
            .Select(review => new AdminReviewDto(
                review.Id,
                review.User != null ? review.User.UserName : "Unknown user",
                review.Movie != null ? review.Movie.Title : review.Show != null ? review.Show.Title : "Unknown content",
                review.Movie != null ? "Movie" : review.Show != null ? "Show" : "Unknown",
                review.Rating,
                review.Comment
            ))
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(item => item.Id == id);

        if (review == null)
        {
            return NotFound();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public sealed record AdminReviewDto(
    int Id,
    string User,
    string Content,
    string Type,
    int Rating,
    string Comment
);
