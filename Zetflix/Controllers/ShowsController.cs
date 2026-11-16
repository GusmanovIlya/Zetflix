using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Services;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class ShowsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ShowsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index(string? search, int? genreId, int page = 1)
    {
        var showsQuery = _context.Shows
            .Include(show => show.ShowGenres)
                .ThenInclude(showGenre => showGenre.Genre)
            .Include(show => show.Seasons)
                .ThenInclude(season => season.Episodes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            showsQuery = showsQuery.Where(show => show.Title.ToLower().Contains(search.ToLower()));
        }

        if (genreId.HasValue)
        {
            showsQuery = showsQuery.Where(show => show.ShowGenres.Any(showGenre => showGenre.GenreId == genreId.Value));
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

        var shows = await PaginatedList<Zetflix.Models.Show>.CreateAsync(
            showsQuery.OrderBy(show => show.Title),
            page,
            8);

        return View(shows);
    }

    public async Task<IActionResult> Details(int id)
    {
        var show = await _context.Shows
            .Include(currentShow => currentShow.ShowGenres)
                .ThenInclude(showGenre => showGenre.Genre)
            .Include(currentShow => currentShow.Seasons)
                .ThenInclude(season => season.Episodes)
            .FirstOrDefaultAsync(currentShow => currentShow.Id == id);

        if (show == null)
        {
            return NotFound();
        }

        var model = new ShowDetailsViewModel
        {
            Show = show,
            Seasons = show.Seasons
                .OrderBy(season => season.Number)
                .Select(season =>
                {
                    season.Episodes = season.Episodes
                        .OrderBy(episode => episode.EpisodeNumber)
                        .ToList();
                    return season;
                })
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Watch(int id)
    {
        var episode = await _context.Episodes
            .Include(currentEpisode => currentEpisode.Season)
                .ThenInclude(season => season!.Show)
            .FirstOrDefaultAsync(currentEpisode => currentEpisode.Id == id);

        if (episode?.Season?.Show == null)
        {
            return NotFound();
        }

        var seasonEpisodes = await _context.Episodes
            .Where(currentEpisode => currentEpisode.SeasonId == episode.SeasonId)
            .OrderBy(currentEpisode => currentEpisode.EpisodeNumber)
            .ToListAsync();

        episode.VideoUrl = PlayableVideoUrl.ResolveForPlayback(episode.VideoUrl, _environment);

        var model = new EpisodeWatchViewModel
        {
            Show = episode.Season.Show,
            Season = episode.Season,
            Episode = episode,
            SeasonEpisodes = seasonEpisodes
        };

        return View(model);
    }
}
