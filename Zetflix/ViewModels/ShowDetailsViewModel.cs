using Zetflix.Models;

namespace Zetflix.ViewModels;

public class ShowDetailsViewModel
{
    public Show Show { get; set; } = new();
    public List<Season> Seasons { get; set; } = new();
}
