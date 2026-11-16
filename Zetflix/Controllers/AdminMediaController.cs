using Microsoft.AspNetCore.Mvc;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/media")]
public class AdminMediaController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".mp4",
        ".webm"
    };

    private readonly IWebHostEnvironment _environment;

    public AdminMediaController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    public ActionResult<IEnumerable<AdminMediaFileDto>> GetFiles()
    {
        var uploadDirectory = GetUploadDirectory();

        if (!Directory.Exists(uploadDirectory))
        {
            return Ok(Array.Empty<AdminMediaFileDto>());
        }

        var files = Directory
            .EnumerateFiles(uploadDirectory)
            .Select(filePath =>
            {
                var file = new FileInfo(filePath);
                return new AdminMediaFileDto(
                    file.Name,
                    $"/uploads/{file.Name}",
                    GetContentType(file.Extension),
                    file.Length,
                    file.LastWriteTimeUtc
                );
            })
            .OrderByDescending(file => file.LastModifiedUtc)
            .ToList();

        return Ok(files);
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<AdminMediaFileDto>> UploadFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest("Unsupported file type.");
        }

        var uploadDirectory = GetUploadDirectory();
        Directory.CreateDirectory(uploadDirectory);

        var safeName = Path.GetFileNameWithoutExtension(file.FileName)
            .ToLowerInvariant()
            .Replace(" ", "-");
        var fileName = $"{safeName}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var uploadedFile = new FileInfo(filePath);

        return Ok(new AdminMediaFileDto(
            uploadedFile.Name,
            $"/uploads/{uploadedFile.Name}",
            GetContentType(uploadedFile.Extension),
            uploadedFile.Length,
            uploadedFile.LastWriteTimeUtc
        ));
    }

    [HttpDelete("{fileName}")]
    public IActionResult DeleteFile(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(GetUploadDirectory(), safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        System.IO.File.Delete(filePath);
        return NoContent();
    }

    private string GetUploadDirectory()
    {
        return Path.Combine(_environment.WebRootPath, "uploads");
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            _ => "application/octet-stream"
        };
    }
}

public sealed record AdminMediaFileDto(
    string FileName,
    string Url,
    string ContentType,
    long Size,
    DateTime LastModifiedUtc
);
