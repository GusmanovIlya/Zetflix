using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Zetflix.Controllers;

public class CultureController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        var supportedCulture = culture == "en" ? "en" : "ru";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(supportedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}
