using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/shows/{showId:int}/seasons")]
public class AdminSeasonsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminSeasonsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminSeasonDto>>> GetSeasons(int showId)
    {
        var showExists = await _context.Shows
            .AsNoTracking()
            .AnyAsync(show => show.Id == showId);

        if (!showExists)
        {
            return NotFound("Show was not found.");
        }

        var seasons = await _context.Seasons
            .AsNoTracking()
            .Where(season => season.ShowId == showId)
            .Include(season => season.Episodes)
            .OrderBy(season => season.Number)
            .Select(season => new AdminSeasonDto(
                season.Id,
                season.Number,
                season.ShowId,
                season.Episodes.Count
            ))
            .ToListAsync();

        return Ok(seasons);
    }

    [HttpPost]
    public async Task<ActionResult<AdminSeasonDto>> CreateSeason(
        int showId,
        CreateAdminSeasonDto request)
    {
        var showExists = await _context.Shows
            .AsNoTracking()
            .AnyAsync(show => show.Id == showId);

        if (!showExists)
        {
            return NotFound("Show was not found.");
        }

        if (request.Number <= 0)
        {
            return BadRequest("Season number must be greater than zero.");
        }

        var duplicateExists = await _context.Seasons
            .AnyAsync(season => season.ShowId == showId && season.Number == request.Number);

        if (duplicateExists)
        {
            return BadRequest("Season with this number already exists.");
        }

        var season = new Season
        {
            Number = request.Number,
            ShowId = showId
        };

        _context.Seasons.Add(season);
        await _context.SaveChangesAsync();

        var createdSeason = new AdminSeasonDto(
            season.Id,
            season.Number,
            season.ShowId,
            0
        );

        return CreatedAtAction(
            nameof(GetSeasons),
            new { showId },
            createdSeason
        );
    }
}

public sealed record AdminSeasonDto(
    int Id,
    int Number,
    int ShowId,
    int EpisodesCount
);

public sealed record CreateAdminSeasonDto(
    int Number
);
