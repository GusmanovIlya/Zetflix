using Zetflix.Models;

namespace Zetflix.ViewModels;

public class HomeIndexViewModel
{
    public SiteSettings Settings { get; set; } = SiteSettings.Default;
    public int MovieCount { get; set; }
    public int ShowCount { get; set; }
    public int GenreCount { get; set; }
    public List<Movie> FeaturedMovies { get; set; } = new();
    public List<Show> FeaturedShows { get; set; } = new();
}
