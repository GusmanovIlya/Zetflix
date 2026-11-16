using Microsoft.AspNetCore.Hosting;

namespace Zetflix.Services;

public static class PlayableVideoUrl
{
    public const string Default = "/assets/images/video/sample-video.mp4";

    public static string NormalizeForStorage(string? videoUrl)
    {
        var normalized = videoUrl?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Default;
        }

        if (IsRemoteUrl(normalized))
        {
            return HasSupportedExtension(normalized) ? normalized : Default;
        }

        normalized = "/" + normalized.TrimStart('/');

        if (normalized.StartsWith("/videos/", StringComparison.OrdinalIgnoreCase))
        {
            return Default;
        }

        return HasSupportedExtension(normalized) ? normalized : Default;
    }

    public static string ResolveForPlayback(string? videoUrl, IWebHostEnvironment environment)
    {
        var normalized = NormalizeForStorage(videoUrl);

        if (IsRemoteUrl(normalized))
        {
            return normalized;
        }

        var pathOnly = normalized.Split('?', '#')[0].TrimStart('/');
        var physicalPath = Path.Combine(
            environment.WebRootPath,
            pathOnly.Replace('/', Path.DirectorySeparatorChar));

        return File.Exists(physicalPath) ? normalized : Default;
    }

    public static string GetContentType(string videoUrl)
    {
        return videoUrl.Split('?', '#')[0].EndsWith(".webm", StringComparison.OrdinalIgnoreCase)
            ? "video/webm"
            : "video/mp4";
    }

    private static bool IsRemoteUrl(string videoUrl)
    {
        return Uri.TryCreate(videoUrl, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool HasSupportedExtension(string videoUrl)
    {
        var pathOnly = videoUrl.Split('?', '#')[0];

        return pathOnly.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
            pathOnly.EndsWith(".webm", StringComparison.OrdinalIgnoreCase);
    }
}
