using Zetflix.Models;

namespace Zetflix.ViewModels;

public class VideoIndexViewModel
{
    public List<Movie> Movies { get; set; } = new();
    public List<Episode> Episodes { get; set; } = new();
    public string? Search { get; set; }
    public string Type { get; set; } = "all";

    public bool HasVideos => Movies.Any() || Episodes.Any();
}
