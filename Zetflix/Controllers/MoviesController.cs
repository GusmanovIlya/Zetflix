using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Services;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public MoviesController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index(string? search, int? genreId, int page = 1)
    {
        var moviesQuery = _context.Movies
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            moviesQuery = moviesQuery.Where(movie => movie.Title.ToLower().Contains(search.ToLower()));
        }

        if (genreId.HasValue)
        {
            moviesQuery = moviesQuery.Where(movie => movie.MovieGenres.Any(movieGenre => movieGenre.GenreId == genreId.Value));
        }

        ViewBag.Search = search;
        ViewBag.GenreId = genreId;
        ViewBag.Genres = await _context.Genres
            .OrderBy(genre => genre.Name)
            .Select(genre => new SelectListItem
            {
                Value = genre.Id.ToString(),
                Text = genre.Name,
                Selected = genre.Id == genreId
            })
            .ToListAsync();

        var movies = await PaginatedList<Zetflix.Models.Movie>.CreateAsync(
            moviesQuery.OrderBy(movie => movie.Title),
            page,
            8);

        var movieIds = movies.Select(movie => movie.Id).ToList();
        ViewBag.MovieRatings = await _context.Reviews
            .AsNoTracking()
            .Where(review => review.MovieId.HasValue && movieIds.Contains(review.MovieId.Value))
            .GroupBy(review => review.MovieId!.Value)
            .Select(group => new
            {
                MovieId = group.Key,
                AverageRating = group.Average(review => review.Rating),
                ReviewCount = group.Count()
            })
            .ToDictionaryAsync(
                item => item.MovieId,
                item => new ContentRatingViewModel(Math.Round(item.AverageRating, 1), item.ReviewCount));

        return View(movies);
    }

    public async Task<IActionResult> Details(int id)
    {
        var movie = await _context.Movies
            .Include(currentMovie => currentMovie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .FirstOrDefaultAsync(currentMovie => currentMovie.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        var rating = await _context.Reviews
            .AsNoTracking()
            .Where(review => review.MovieId == movie.Id)
            .GroupBy(review => review.MovieId)
            .Select(group => new ContentRatingViewModel(
                Math.Round(group.Average(review => review.Rating), 1),
                group.Count()))
            .FirstOrDefaultAsync();

        //нужен, чтобы после фильтрации input сохранил введенный текст.
        ViewBag.Rating = rating ?? new ContentRatingViewModel(0, 0);

        return View(movie);
    }

    public async Task<IActionResult> Watch(int id)
    {
        var movie = await _context.Movies
            .Include(currentMovie => currentMovie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .FirstOrDefaultAsync(currentMovie => currentMovie.Id == id);

        if (movie == null)
        {
            return NotFound();
        }

        movie.VideoUrl = PlayableVideoUrl.ResolveForPlayback(movie.VideoUrl, _environment);

        return View(movie);
    }
}
