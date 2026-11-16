using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/shows")]
public class AdminShowsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminShowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminShowDto>>> GetShows()
    {
        var shows = await _context.Shows
            .AsNoTracking()
            .Include(show => show.ShowGenres)
                .ThenInclude(showGenre => showGenre.Genre)
            .Include(show => show.Seasons)
                .ThenInclude(season => season.Episodes)
            .OrderBy(show => show.Title)
            .Select(show => new AdminShowDto(
                show.Id,
                show.Title,
                show.Description,
                show.ReleaseYear,
                show.PosterUrl,
                show.Seasons.Count,
                show.Seasons.SelectMany(season => season.Episodes).Count(),
                show.ShowGenres
                    .Where(showGenre => showGenre.Genre != null)
                    .Select(showGenre => showGenre.Genre!.Name)
                    .OrderBy(name => name)
                    .ToArray()
            ))
            .ToListAsync();

        return Ok(shows);
    }

    [HttpPost]
    public async Task<ActionResult<AdminShowDto>> CreateShow(CreateAdminShowDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest("Description is required.");
        }

        var show = new Show
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ReleaseYear = request.ReleaseYear,
            PosterUrl = string.IsNullOrWhiteSpace(request.PosterUrl)
                ? "/images/shows/default.jpg"
                : request.PosterUrl.Trim()
        };

        var genreIds = await _context.Genres
            .Where(genre => request.GenreIds.Contains(genre.Id))
            .Select(genre => genre.Id)
            .ToListAsync();

        foreach (var genreId in genreIds)
        {
            show.ShowGenres.Add(new ShowGenre
            {
                GenreId = genreId
            });
        }

        _context.Shows.Add(show);
        await _context.SaveChangesAsync();

        var createdShow = await GetShowDto(show.Id);

        return CreatedAtAction(nameof(GetShows), new { id = createdShow.Id }, createdShow);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminShowDto>> UpdateShow(int id, UpdateAdminShowDto request)
    {
        var show = await _context.Shows
            .Include(item => item.ShowGenres)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (show == null)
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

        show.Title = request.Title.Trim();
        show.Description = request.Description.Trim();
        show.ReleaseYear = request.ReleaseYear;
        show.PosterUrl = string.IsNullOrWhiteSpace(request.PosterUrl)
            ? "/images/shows/default.jpg"
            : request.PosterUrl.Trim();

        _context.ShowGenres.RemoveRange(show.ShowGenres);

        var genreIds = await _context.Genres
            .Where(genre => request.GenreIds.Contains(genre.Id))
            .Select(genre => genre.Id)
            .ToListAsync();

        foreach (var genreId in genreIds)
        {
            show.ShowGenres.Add(new ShowGenre
            {
                ShowId = show.Id,
                GenreId = genreId
            });
        }

        await _context.SaveChangesAsync();

        return Ok(await GetShowDto(show.Id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteShow(int id)
    {
        var show = await _context.Shows
            .Include(item => item.ShowGenres)
            .Include(item => item.Seasons)
                .ThenInclude(season => season.Episodes)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (show == null)
        {
            return NotFound();
        }

        var reviews = await _context.Reviews
            .Where(review => review.ShowId == id)
            .ToListAsync();
        var episodes = show.Seasons.SelectMany(season => season.Episodes).ToList();

        _context.Reviews.RemoveRange(reviews);
        _context.Episodes.RemoveRange(episodes);
        _context.Seasons.RemoveRange(show.Seasons);
        _context.ShowGenres.RemoveRange(show.ShowGenres);
        _context.Shows.Remove(show);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<AdminShowDto> GetShowDto(int id)
    {
        return await _context.Shows
            .AsNoTracking()
            .Include(show => show.ShowGenres)
                .ThenInclude(showGenre => showGenre.Genre)
            .Include(show => show.Seasons)
                .ThenInclude(season => season.Episodes)
            .Where(show => show.Id == id)
            .Select(show => new AdminShowDto(
                show.Id,
                show.Title,
                show.Description,
                show.ReleaseYear,
                show.PosterUrl,
                show.Seasons.Count,
                show.Seasons.SelectMany(season => season.Episodes).Count(),
                show.ShowGenres
                    .Where(showGenre => showGenre.Genre != null)
                    .Select(showGenre => showGenre.Genre!.Name)
                    .OrderBy(name => name)
                    .ToArray()
            ))
            .FirstAsync();
    }
}

public sealed record AdminShowDto(
    int Id,
    string Title,
    string Description,
    int ReleaseYear,
    string PosterUrl,
    int SeasonsCount,
    int EpisodesCount,
    IReadOnlyCollection<string> Genres
);

public sealed record CreateAdminShowDto(
    string Title,
    string Description,
    int ReleaseYear,
    string PosterUrl,
    IReadOnlyCollection<int> GenreIds
);

public sealed record UpdateAdminShowDto(
    string Title,
    string Description,
    int ReleaseYear,
    string PosterUrl,
    IReadOnlyCollection<int> GenreIds
);
