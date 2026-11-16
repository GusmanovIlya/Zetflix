using Zetflix.Models;

namespace Zetflix.ViewModels;

public class EpisodeWatchViewModel
{
    public Show Show { get; set; } = new();
    public Season Season { get; set; } = new();
    public Episode Episode { get; set; } = new();
    public List<Episode> SeasonEpisodes { get; set; } = new();
}
