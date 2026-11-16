using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Services;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ISiteSettingsService _settingsService;

    public HomeController(ApplicationDbContext context, ISiteSettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            Settings = await _settingsService.GetAsync(),
            MovieCount = await _context.Movies.CountAsync(),
            ShowCount = await _context.Shows.CountAsync(),
            GenreCount = await _context.Genres.CountAsync(),
            FeaturedMovies = await _context.Movies
                .Include(movie => movie.MovieGenres)
                    .ThenInclude(movieGenre => movieGenre.Genre)
                .OrderBy(movie => movie.Title)
                .Take(4)
                .ToListAsync(),
            FeaturedShows = await _context.Shows
                .Include(show => show.ShowGenres)
                    .ThenInclude(showGenre => showGenre.Genre)
                .Include(show => show.Seasons)
                    .ThenInclude(season => season.Episodes)
                .OrderBy(show => show.Title)
                .Take(4)
                .ToListAsync()
        };

        return View(model);
    }
}
