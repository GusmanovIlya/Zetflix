using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/analytics")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminAnalyticsController> _logger;

    public AdminAnalyticsController(ApplicationDbContext context, ILogger<AdminAnalyticsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AdminAnalyticsDto>> GetAnalytics()
    {
        try
        {
            var movies = await _context.Movies.AsNoTracking().ToListAsync();
            var shows = await _context.Shows.AsNoTracking().ToListAsync();
            var seasons = await _context.Seasons.AsNoTracking().ToListAsync();
            var episodes = await _context.Episodes.AsNoTracking().ToListAsync();
            var users = await _context.Users.AsNoTracking().ToListAsync();
            var roles = await _context.Roles.AsNoTracking().ToListAsync();
            var reviews = await _context.Reviews.AsNoTracking().ToListAsync();
            var genres = await _context.Genres.AsNoTracking().ToListAsync();
            var movieGenres = await _context.MovieGenres.AsNoTracking().ToListAsync();
            var showGenres = await _context.ShowGenres.AsNoTracking().ToListAsync();
            var subscriptions = await _context.Subscriptions.AsNoTracking().ToListAsync();

            var movieCount = movies.Count;
            var showCount = shows.Count;
            var episodeCount = episodes.Count;
            var userCount = users.Count;
            var reviewCount = reviews.Count;
            var revenue = users
                .Where(user => user.SubscriptionId.HasValue)
                .Join(
                    subscriptions,
                    user => user.SubscriptionId!.Value,
                    subscription => subscription.Id,
                    (_, subscription) => subscription.Price)
                .Sum();
            var averageRating = reviews.Count > 0 ? reviews.Average(review => review.Rating) : 0;

            var genreBreakdown = genres
                .Select(genre => new AdminAnalyticsGenreDto(
                    genre.Name,
                    movieGenres.Count(movieGenre => movieGenre.GenreId == genre.Id),
                    showGenres.Count(showGenre => showGenre.GenreId == genre.Id)))
                .OrderByDescending(genre => genre.MovieCount + genre.ShowCount)
                .ToList();

            var ratingDistribution = reviews
                .GroupBy(review => review.Rating)
                .Select(group => new AdminRatingAnalyticsDto(group.Key, group.Count()))
                .OrderByDescending(rating => rating.Rating)
                .ToList();

            var subscriptionBreakdown = subscriptions
                .Select(subscription =>
                {
                    var currentUserCount = users.Count(user => user.SubscriptionId == subscription.Id);

                    return new AdminSubscriptionAnalyticsDto(
                        subscription.Name,
                        subscription.Price,
                        currentUserCount,
                        currentUserCount * subscription.Price);
                })
                .OrderByDescending(subscription => subscription.Revenue)
                .ToList();

            var roleRows = users
                .GroupBy(user => roles.FirstOrDefault(role => role.Id == user.RoleId)?.Name ?? "No role")
                .Select(group => new AdminRoleAnalyticsDto(group.Key, group.Count()))
                .OrderByDescending(role => role.UserCount)
                .ToList();

            var moderation = new AdminModerationAnalyticsDto(
                reviewCount,
                reviews.Count(review => review.Rating <= 2),
                reviews.Count(review => !string.IsNullOrWhiteSpace(review.Comment)));

            var moviesWithPoster = movies.Count(movie => !string.IsNullOrWhiteSpace(movie.PosterUrl));
            var showsWithPoster = shows.Count(show => !string.IsNullOrWhiteSpace(show.PosterUrl));
            var showsWithEpisodes = seasons
                .Where(season => episodes.Any(episode => episode.SeasonId == season.Id))
                .Select(season => season.ShowId)
                .Distinct()
                .Count();
            var reviewedContent = reviews
                .Select(review => review.MovieId.HasValue ? $"movie-{review.MovieId}" : $"show-{review.ShowId}")
                .Distinct()
                .Count();

            var totalContent = movieCount + showCount;

            var contentHealth = new[]
            {
                new AdminContentHealthDto("Movies with posters", moviesWithPoster, movieCount),
                new AdminContentHealthDto("Shows with posters", showsWithPoster, showCount),
                new AdminContentHealthDto("Shows with episodes", showsWithEpisodes, showCount),
                new AdminContentHealthDto("Reviewed content", reviewedContent, totalContent)
            };

            var summary = new[]
            {
                new AdminAnalyticsMetricDto("Catalog items", totalContent, "Movies + shows"),
                new AdminAnalyticsMetricDto("Episodes", episodeCount, "Playable series entries"),
                new AdminAnalyticsMetricDto("Users", userCount, "Registered accounts"),
                new AdminAnalyticsMetricDto("Reviews", reviewCount, $"{Math.Round(averageRating, 1)} average rating"),
                new AdminAnalyticsMetricDto("Revenue", revenue, "Assigned subscriptions")
            };

            return Ok(new AdminAnalyticsDto(
                summary,
                genreBreakdown,
                ratingDistribution,
                subscriptionBreakdown,
                contentHealth,
                roleRows,
                moderation
            ));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Admin analytics API failed.");

            return Ok(new AdminAnalyticsDto(
                Array.Empty<AdminAnalyticsMetricDto>(),
                Array.Empty<AdminAnalyticsGenreDto>(),
                Array.Empty<AdminRatingAnalyticsDto>(),
                Array.Empty<AdminSubscriptionAnalyticsDto>(),
                Array.Empty<AdminContentHealthDto>(),
                Array.Empty<AdminRoleAnalyticsDto>(),
                new AdminModerationAnalyticsDto(0, 0, 0)
            ));
        }
    }
}

public sealed record AdminAnalyticsDto(
    IReadOnlyCollection<AdminAnalyticsMetricDto> Summary,
    IReadOnlyCollection<AdminAnalyticsGenreDto> Genres,
    IReadOnlyCollection<AdminRatingAnalyticsDto> Ratings,
    IReadOnlyCollection<AdminSubscriptionAnalyticsDto> Subscriptions,
    IReadOnlyCollection<AdminContentHealthDto> ContentHealth,
    IReadOnlyCollection<AdminRoleAnalyticsDto> Roles,
    AdminModerationAnalyticsDto Moderation
);

public sealed record AdminAnalyticsMetricDto(string Label, decimal Value, string Hint);

public sealed record AdminAnalyticsGenreDto(string Name, int MovieCount, int ShowCount);

public sealed record AdminRatingAnalyticsDto(int Rating, int Count);

public sealed record AdminSubscriptionAnalyticsDto(
    string Name,
    decimal Price,
    int UserCount,
    decimal Revenue
);

public sealed record AdminContentHealthDto(string Label, int Ready, int Total);

public sealed record AdminRoleAnalyticsDto(string Role, int UserCount);

public sealed record AdminModerationAnalyticsDto(int TotalReviews, int LowRatingReviews, int ReviewsWithComments);
