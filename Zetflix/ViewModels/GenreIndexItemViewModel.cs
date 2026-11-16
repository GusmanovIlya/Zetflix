namespace Zetflix.ViewModels;

public class GenreIndexItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MovieCount { get; set; }
    public int ShowCount { get; set; }
}
