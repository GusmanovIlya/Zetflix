using Microsoft.AspNetCore.Mvc;
using Zetflix.Models;
using Zetflix.Services;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/settings")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISiteSettingsService _settingsService;

    public AdminSettingsController(ISiteSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<ActionResult<SiteSettings>> GetSettings()
    {
        return Ok(await _settingsService.GetAsync());
    }

    [HttpPut]
    public async Task<ActionResult<SiteSettings>> UpdateSettings(SiteSettings request)
    {
        if (string.IsNullOrWhiteSpace(request.SiteName))
        {
            return BadRequest("Site name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SupportEmail))
        {
            return BadRequest("Support email is required.");
        }

        var settings = await _settingsService.SaveAsync(new SiteSettings(
            request.SiteName,
            request.SupportEmail,
            request.HeroTitle,
            request.HeroSubtitle,
            request.AllowRegistration,
            request.MaintenanceMode
        ));

        return Ok(settings);
    }
}
