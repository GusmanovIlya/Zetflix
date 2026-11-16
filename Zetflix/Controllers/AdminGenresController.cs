using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/genres")]
public class AdminGenresController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminGenresController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminGenreDto>>> GetGenres()
    {
        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .Select(genre => new AdminGenreDto(
                genre.Id,
                genre.Name
            ))
            .ToListAsync();

        return Ok(genres);
    }

    [HttpPost]
    public async Task<ActionResult<AdminGenreDto>> CreateGenre(CreateAdminGenreDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var name = request.Name.Trim();
        var duplicateExists = await _context.Genres
            .AnyAsync(genre => genre.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            return BadRequest("Genre already exists.");
        }

        var genre = new Genre
        {
            Name = name
        };

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();

        var createdGenre = new AdminGenreDto(
            genre.Id,
            genre.Name
        );

        return CreatedAtAction(nameof(GetGenres), new { id = genre.Id }, createdGenre);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminGenreDto>> UpdateGenre(int id, UpdateAdminGenreDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var genre = await _context.Genres
            .FirstOrDefaultAsync(item => item.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();
        var duplicateExists = await _context.Genres
            .AnyAsync(item => item.Id != id && item.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            return BadRequest("Genre already exists.");
        }

        genre.Name = name;
        await _context.SaveChangesAsync();

        return Ok(new AdminGenreDto(
            genre.Id,
            genre.Name
        ));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await _context.Genres
            .Include(item => item.MovieGenres)
            .Include(item => item.ShowGenres)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (genre == null)
        {
            return NotFound();
        }

        if (genre.MovieGenres.Count > 0 || genre.ShowGenres.Count > 0)
        {
            return BadRequest("Genre is used by movies or shows.");
        }

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public sealed record AdminGenreDto(
    int Id,
    string Name
);

public sealed record CreateAdminGenreDto(
    string Name
);

public sealed record UpdateAdminGenreDto(
    string Name
);
