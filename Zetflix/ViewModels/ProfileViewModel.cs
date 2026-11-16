using Zetflix.Models;

namespace Zetflix.ViewModels;

public class ProfileViewModel
{
    public User User { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}
