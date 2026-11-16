namespace Zetflix.Models;

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
