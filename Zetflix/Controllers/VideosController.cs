using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class VideosController : Controller
{
    private readonly ApplicationDbContext _context;

    public VideosController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string type = "all")
    {
        var normalizedType = type is "movies" or "episodes" ? type : "all";

        var moviesQuery = _context.Movies
            .Include(movie => movie.MovieGenres)
                .ThenInclude(movieGenre => movieGenre.Genre)
            .Where(movie => movie.VideoUrl != "");

        var episodesQuery = _context.Episodes
            .Include(episode => episode.Season)
                .ThenInclude(season => season!.Show)
            .Where(episode => episode.VideoUrl != "");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLower();
            moviesQuery = moviesQuery.Where(movie =>
                movie.Title.ToLower().Contains(normalizedSearch) ||
                movie.Description.ToLower().Contains(normalizedSearch));
            episodesQuery = episodesQuery.Where(episode =>
                episode.Title.ToLower().Contains(normalizedSearch) ||
                episode.Description.ToLower().Contains(normalizedSearch) ||
                episode.Season!.Show!.Title.ToLower().Contains(normalizedSearch));
        }

        var model = new VideoIndexViewModel
        {
            Search = search,
            Type = normalizedType,
            Movies = normalizedType == "episodes"
                ? new()
                : await moviesQuery
                .OrderBy(movie => movie.Title)
                .Take(12)
                .ToListAsync(),
            Episodes = normalizedType == "movies"
                ? new()
                : await episodesQuery
                .OrderBy(episode => episode.Season!.Show!.Title)
                .ThenBy(episode => episode.Season!.Number)
                .ThenBy(episode => episode.EpisodeNumber)
                .Take(12)
                .ToListAsync()
        };

        return View(model);
    }
}
