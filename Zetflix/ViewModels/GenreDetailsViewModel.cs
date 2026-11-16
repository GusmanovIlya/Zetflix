using Zetflix.Models;

namespace Zetflix.ViewModels;

public class GenreDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Movie> Movies { get; set; } = new();
    public List<Show> Shows { get; set; } = new();
}
