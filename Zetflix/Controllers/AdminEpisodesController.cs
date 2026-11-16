using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;
using Zetflix.Services;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/seasons/{seasonId:int}/episodes")]
public class AdminEpisodesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminEpisodesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminEpisodeDto>>> GetEpisodes(int seasonId)
    {
        var seasonExists = await _context.Seasons
            .AsNoTracking()
            .AnyAsync(season => season.Id == seasonId);

        if (!seasonExists)
        {
            return NotFound("Season was not found.");
        }

        var episodes = await _context.Episodes
            .AsNoTracking()
            .Where(episode => episode.SeasonId == seasonId)
            .OrderBy(episode => episode.EpisodeNumber)
            .Select(episode => new AdminEpisodeDto(
                episode.Id,
                episode.Title,
                episode.Description,
                episode.EpisodeNumber,
                episode.VideoUrl,
                episode.SeasonId
            ))
            .ToListAsync();

        return Ok(episodes);
    }

    [HttpPost]
    public async Task<ActionResult<AdminEpisodeDto>> CreateEpisode(
        int seasonId,
        CreateAdminEpisodeDto request)
    {
        var seasonExists = await _context.Seasons
            .AsNoTracking()
            .AnyAsync(season => season.Id == seasonId);

        if (!seasonExists)
        {
            return NotFound("Season was not found.");
        }

        var validationError = ValidateEpisode(request.Title, request.Description, request.EpisodeNumber, request.VideoUrl);

        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var duplicateExists = await _context.Episodes
            .AnyAsync(episode => episode.SeasonId == seasonId && episode.EpisodeNumber == request.EpisodeNumber);

        if (duplicateExists)
        {
            return BadRequest("Episode with this number already exists.");
        }

        var episode = new Episode
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            EpisodeNumber = request.EpisodeNumber,
            VideoUrl = PlayableVideoUrl.NormalizeForStorage(request.VideoUrl),
            SeasonId = seasonId
        };

        _context.Episodes.Add(episode);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEpisodes),
            new { seasonId },
            ToDto(episode)
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminEpisodeDto>> UpdateEpisode(
        int seasonId,
        int id,
        UpdateAdminEpisodeDto request)
    {
        var episode = await _context.Episodes
            .FirstOrDefaultAsync(item => item.Id == id && item.SeasonId == seasonId);

        if (episode == null)
        {
            return NotFound();
        }

        var validationError = ValidateEpisode(request.Title, request.Description, request.EpisodeNumber, request.VideoUrl);

        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var duplicateExists = await _context.Episodes
            .AnyAsync(item =>
                item.SeasonId == seasonId &&
                item.EpisodeNumber == request.EpisodeNumber &&
                item.Id != id);

        if (duplicateExists)
        {
            return BadRequest("Episode with this number already exists.");
        }

        episode.Title = request.Title.Trim();
        episode.Description = request.Description.Trim();
        episode.EpisodeNumber = request.EpisodeNumber;
        episode.VideoUrl = PlayableVideoUrl.NormalizeForStorage(request.VideoUrl);

        await _context.SaveChangesAsync();

        return Ok(ToDto(episode));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEpisode(int seasonId, int id)
    {
        var episode = await _context.Episodes
            .FirstOrDefaultAsync(item => item.Id == id && item.SeasonId == seasonId);

        if (episode == null)
        {
            return NotFound();
        }

        _context.Episodes.Remove(episode);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidateEpisode(
        string title,
        string description,
        int episodeNumber,
        string videoUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description is required.";
        }

        if (episodeNumber <= 0)
        {
            return "Episode number must be greater than zero.";
        }

        return null;
    }

    private static AdminEpisodeDto ToDto(Episode episode)
    {
        return new AdminEpisodeDto(
            episode.Id,
            episode.Title,
            episode.Description,
            episode.EpisodeNumber,
            episode.VideoUrl,
            episode.SeasonId
        );
    }
}

public sealed record AdminEpisodeDto(
    int Id,
    string Title,
    string Description,
    int EpisodeNumber,
    string VideoUrl,
    int SeasonId
);

public sealed record CreateAdminEpisodeDto(
    string Title,
    string Description,
    int EpisodeNumber,
    string VideoUrl
);

public sealed record UpdateAdminEpisodeDto(
    string Title,
    string Description,
    int EpisodeNumber,
    string VideoUrl
);
