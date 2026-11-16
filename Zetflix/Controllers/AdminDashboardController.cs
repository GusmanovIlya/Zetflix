using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(ApplicationDbContext context, ILogger<AdminDashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        try
        {
            var movieCount = await _context.Movies.CountAsync();
            var showCount = await _context.Shows.CountAsync();
            var episodeCount = await _context.Episodes.CountAsync();
            var userCount = await _context.Users.CountAsync();
            var reviewCount = await _context.Reviews.CountAsync();
            var subscriptionCount = await _context.Subscriptions.CountAsync();

            var revenue = await _context.Users
                .AsNoTracking()
                .Where(user => user.SubscriptionId != null)
                .Join(
                    _context.Subscriptions.AsNoTracking(),
                    user => user.SubscriptionId,
                    subscription => subscription.Id,
                    (_, subscription) => (decimal?)subscription.Price)
                .SumAsync() ?? 0m;

            var averageRating = await _context.Reviews
                .AsNoTracking()
                .Select(review => (double?)review.Rating)
                .AverageAsync() ?? 0;

            var movieGenreCounts = await _context.MovieGenres
                .AsNoTracking()
                .GroupBy(movieGenre => movieGenre.GenreId)
                .Select(group => new { GenreId = group.Key, Count = group.Count() })
                .ToListAsync();

            var showGenreCounts = await _context.ShowGenres
                .AsNoTracking()
                .GroupBy(showGenre => showGenre.GenreId)
                .Select(group => new { GenreId = group.Key, Count = group.Count() })
                .ToListAsync();

            var genres = await _context.Genres.AsNoTracking().ToListAsync();
            var genreBreakdown = genres
                .Select(genre => new AdminDashboardGenreDto(
                    genre.Name,
                    movieGenreCounts.Where(item => item.GenreId == genre.Id).Sum(item => item.Count) +
                    showGenreCounts.Where(item => item.GenreId == genre.Id).Sum(item => item.Count)))
                .OrderByDescending(genre => genre.Count)
                .Take(5)
                .ToList();

            var recentReviewRows = await _context.Reviews
                .AsNoTracking()
                .Include(review => review.User)
                .Include(review => review.Movie)
                .Include(review => review.Show)
                .OrderByDescending(review => review.Id)
                .Take(4)
                .ToListAsync();

            var recentReviews = recentReviewRows
                .Select(review => new AdminDashboardReviewDto(
                    review.Id,
                    review.User?.UserName ?? "Unknown user",
                    review.Movie?.Title ?? review.Show?.Title ?? "Unknown content",
                    review.Rating))
                .ToList();

            return Ok(new AdminDashboardDto(
                movieCount,
                showCount,
                episodeCount,
                userCount,
                reviewCount,
                subscriptionCount,
                revenue,
                Math.Round(averageRating, 1),
                genreBreakdown,
                recentReviews
            ));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Admin dashboard API failed.");

            return Ok(new AdminDashboardDto(
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                Array.Empty<AdminDashboardGenreDto>(),
                Array.Empty<AdminDashboardReviewDto>()
            ));
        }
    }
}

public sealed record AdminDashboardDto(
    int MovieCount,
    int ShowCount,
    int EpisodeCount,
    int UserCount,
    int ReviewCount,
    int SubscriptionCount,
    decimal Revenue,
    double AverageRating,
    IReadOnlyCollection<AdminDashboardGenreDto> GenreBreakdown,
    IReadOnlyCollection<AdminDashboardReviewDto> RecentReviews
);

public sealed record AdminDashboardGenreDto(string Name, int Count);

public sealed record AdminDashboardReviewDto(
    int Id,
    string User,
    string Content,
    int Rating
);
