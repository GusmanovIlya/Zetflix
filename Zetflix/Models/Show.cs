namespace Zetflix.Models;

public class Show
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string PosterUrl { get; set; } = string.Empty;

    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public ICollection<ShowGenre> ShowGenres { get; set; } = new List<ShowGenre>();
}
