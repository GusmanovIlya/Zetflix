namespace Zetflix.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public int? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
