using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class GenresController : Controller
{
    private readonly ApplicationDbContext _context;

    public GenresController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .Select(genre => new GenreIndexItemViewModel
            {
                Id = genre.Id,
                Name = genre.Name,
                MovieCount = genre.MovieGenres.Count,
                ShowCount = genre.ShowGenres.Count
            })
            .ToListAsync();

        return View(genres);
    }

    public async Task<IActionResult> Details(int id)
    {
        var genre = await _context.Genres
            .Include(g => g.MovieGenres)
                .ThenInclude(mg => mg.Movie)
            .Include(g => g.ShowGenres)
                .ThenInclude(sg => sg.Show)
                    .ThenInclude(show => show!.Seasons)
                        .ThenInclude(season => season.Episodes)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        var model = new GenreDetailsViewModel
        {
            Id = genre.Id,
            Name = genre.Name,
            Movies = genre.MovieGenres
                .Where(mg => mg.Movie != null)
                .Select(mg => mg.Movie!)
                .ToList(),
            Shows = genre.ShowGenres
                .Where(sg => sg.Show != null)
                .Select(sg => sg.Show!)
                .ToList()
        };

        return View(model);
    }
}
