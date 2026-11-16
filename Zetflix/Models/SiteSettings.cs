namespace Zetflix.Models;

public sealed record SiteSettings(
    string SiteName,
    string SupportEmail,
    string HeroTitle,
    string HeroSubtitle,
    bool AllowRegistration,
    bool MaintenanceMode
)
{
    public static SiteSettings Default { get; } = new(
        "Zetflix",
        "gusmanovsemestrovkaorisgusmano@gmail.com",
        "Unlimited movies, shows and more",
        "Stream curated content from your Zetflix catalog.",
        true,
        false
    );
}
