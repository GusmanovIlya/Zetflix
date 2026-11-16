using System.Text.Json;
using Zetflix.Models;

namespace Zetflix.Services;

public interface ISiteSettingsService
{
    Task<SiteSettings> GetAsync();
    Task<SiteSettings> SaveAsync(SiteSettings settings);
}

public class SiteSettingsService : ISiteSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly IWebHostEnvironment _environment;

    public SiteSettingsService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<SiteSettings> GetAsync()
    {
        var settingsPath = GetSettingsPath();

        if (!File.Exists(settingsPath))
        {
            return SiteSettings.Default;
        }

        var json = await File.ReadAllTextAsync(settingsPath);
        return JsonSerializer.Deserialize<SiteSettings>(json) ?? SiteSettings.Default;
    }

    public async Task<SiteSettings> SaveAsync(SiteSettings settings)
    {
        var normalizedSettings = new SiteSettings(
            settings.SiteName.Trim(),
            settings.SupportEmail.Trim(),
            settings.HeroTitle.Trim(),
            settings.HeroSubtitle.Trim(),
            settings.AllowRegistration,
            settings.MaintenanceMode
        );

        var settingsPath = GetSettingsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);

        var json = JsonSerializer.Serialize(normalizedSettings, JsonOptions);
        await File.WriteAllTextAsync(settingsPath, json);

        return normalizedSettings;
    }

    private string GetSettingsPath()
    {
        return Path.Combine(_environment.ContentRootPath, "App_Data", "admin-settings.json");
    }
}
