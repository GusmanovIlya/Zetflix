using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/episodes")]
public class AdminEpisodeLibraryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminEpisodeLibraryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminEpisodeLibraryDto>>> GetEpisodes()
    {
        var episodes = await _context.Episodes
            .AsNoTracking()
            .Include(episode => episode.Season)
                .ThenInclude(season => season!.Show)
            .OrderBy(episode => episode.Season!.Show!.Title)
            .ThenBy(episode => episode.Season!.Number)
            .ThenBy(episode => episode.EpisodeNumber)
            .Select(episode => new AdminEpisodeLibraryDto(
                episode.Id,
                episode.Title,
                episode.Description,
                episode.EpisodeNumber,
                episode.VideoUrl,
                episode.SeasonId,
                episode.Season != null ? episode.Season.Number : 0,
                episode.Season != null ? episode.Season.ShowId : 0,
                episode.Season != null && episode.Season.Show != null ? episode.Season.Show.Title : "Unknown show"
            ))
            .ToListAsync();

        return Ok(episodes);
    }
}

public sealed record AdminEpisodeLibraryDto(
    int Id,
    string Title,
    string Description,
    int EpisodeNumber,
    string VideoUrl,
    int SeasonId,
    int SeasonNumber,
    int ShowId,
    string ShowTitle
);
