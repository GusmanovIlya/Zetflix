using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;
using Zetflix.Services;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/movies")]
public class AdminMoviesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminMoviesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminMovieDto>>> GetMovies()
    {
        var movies = await _context.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .OrderBy(movie => movie.Title)
            .ToListAsync();

        var ratings = await GetMovieRatingsAsync(movies.Select(movie => movie.Id));

        return Ok(movies.Select(movie => ToDto(movie, ratings)));
    }

    [HttpPost]
    public async Task<ActionResult<AdminMovieDto>> CreateMovie(CreateAdminMovieDto request)
    {
        var movie = new Movie
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ReleaseYear = request.ReleaseYear,
            PosterUrl = request.PosterUrl.Trim(),
            VideoUrl = PlayableVideoUrl.NormalizeForStorage(request.VideoUrl)
        };

        if (request.GenreIds.Count > 0)
        {
            var genreIds = await _context.Genres
                .Where(genre => request.GenreIds.Contains(genre.Id))
                .Select(genre => genre.Id)
                .ToListAsync();

            foreach (var genreId in genreIds)
            {
                movie.MovieGenres.Add(new MovieGenre
                {
                    GenreId = genreId
                });
            }
        }

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        var createdMovie = await _context.Movies
            .AsNoTracking()
            .Include(item => item.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Where(item => item.Id == movie.Id)
            .FirstAsync();

        var ratings = await GetMovieRatingsAsync(new[] { createdMovie.Id });

        return CreatedAtAction(nameof(GetMovies), new { id = createdMovie.Id }, ToDto(createdMovie, ratings));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var movie = await _context.Movies
            .Include(item => item.MovieGenres)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        var reviews = await _context.Reviews
            .Where(review => review.MovieId == id)
            .ToListAsync();

        _context.Reviews.RemoveRange(reviews);
        _context.MovieGenres.RemoveRange(movie.MovieGenres);
        _context.Movies.Remove(movie);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminMovieDto>> UpdateMovie(int id, UpdateAdminMovieDto request)
    {
        var movie = await _context.Movies
            .Include(item => item.MovieGenres)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest("Description is required.");
        }

        movie.Title = request.Title.Trim();
        movie.Description = request.Description.Trim();
        movie.ReleaseYear = request.ReleaseYear;
        movie.PosterUrl = string.IsNullOrWhiteSpace(request.PosterUrl)
            ? "/images/movies/default.jpg"
            : request.PosterUrl.Trim();
        movie.VideoUrl = PlayableVideoUrl.NormalizeForStorage(request.VideoUrl);

        _context.MovieGenres.RemoveRange(movie.MovieGenres);

        var genreIds = await _context.Genres
            .Where(genre => request.GenreIds.Contains(genre.Id))
            .Select(genre => genre.Id)
            .ToListAsync();

        foreach (var genreId in genreIds)
        {
            movie.MovieGenres.Add(new MovieGenre
            {
                MovieId = movie.Id,
                GenreId = genreId
            });
        }

        await _context.SaveChangesAsync();

        var updatedMovie = await _context.Movies
            .AsNoTracking()
            .Include(item => item.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Where(item => item.Id == movie.Id)
            .FirstAsync();

        var ratings = await GetMovieRatingsAsync(new[] { updatedMovie.Id });

        return Ok(ToDto(updatedMovie, ratings));
    }

    private async Task<Dictionary<int, AdminMovieRatingDto>> GetMovieRatingsAsync(IEnumerable<int> movieIds)
    {
        var ids = movieIds.ToList();

        return await _context.Reviews
            .AsNoTracking()
            .Where(review => review.MovieId.HasValue && ids.Contains(review.MovieId.Value))
            .GroupBy(review => review.MovieId!.Value)
            .Select(group => new
            {
                MovieId = group.Key,
                AverageRating = group.Average(review => review.Rating),
                ReviewCount = group.Count()
            })
            .ToDictionaryAsync(
                item => item.MovieId,
                item => new AdminMovieRatingDto(Math.Round(item.AverageRating, 1), item.ReviewCount));
    }

    private static AdminMovieDto ToDto(Movie movie, IReadOnlyDictionary<int, AdminMovieRatingDto> ratings)
    {
        var rating = ratings.TryGetValue(movie.Id, out var currentRating)
            ? currentRating
            : new AdminMovieRatingDto(0, 0);

        return new AdminMovieDto(
            movie.Id,
            movie.Title,
            movie.Description,
            movie.ReleaseYear,
            movie.PosterUrl,
            movie.VideoUrl,
            movie.MovieGenres
                .Where(movieGenre => movieGenre.Genre != null)
                .Select(movieGenre => movieGenre.Genre!.Name)
                .OrderBy(name => name)
                .ToArray(),
            rating.AverageRating,
            rating.ReviewCount);
    }

}

public sealed record AdminMovieRatingDto(double AverageRating, int ReviewCount);

public sealed record AdminMovieDto(
    int Id,
    string Title,
    string Description,
    int ReleaseYear,
    string PosterUrl,
    string VideoUrl,
    IReadOnlyCollection<string> Genres,
    double AverageRating,
    int ReviewCount
);

public sealed record CreateAdminMovieDto(
    string Title,
    string Description,
    int ReleaseYear,
    string PosterUrl,
    string VideoUrl,
    IReadOnlyCollection<int> GenreIds
);

public sealed record UpdateAdminMovieDto(
    string Title,
    string Description,
    int ReleaseYear,
    string PosterUrl,
    string VideoUrl,
    IReadOnlyCollection<int> GenreIds
);
