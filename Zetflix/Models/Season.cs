namespace Zetflix.Models;

public class Season
{
    public int Id { get; set; }
    public int Number { get; set; }

    public int ShowId { get; set; }
    public Show? Show { get; set; }

    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
