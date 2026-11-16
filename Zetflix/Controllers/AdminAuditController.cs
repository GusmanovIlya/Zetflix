using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Zetflix.Models;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/audit")]
public class AdminAuditController : ControllerBase
{
    private readonly MongoAuditSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminAuditController> _logger;
    private readonly IMongoCollection<AuditEvent>? _auditEvents;

    public AdminAuditController(
        IOptions<MongoAuditSettings> options,
        IWebHostEnvironment environment,
        ILogger<AdminAuditController> logger)
    {
        _settings = options.Value;
        _environment = environment;
        _logger = logger;

        if (_settings.Enabled)
        {
            var mongoUrl = MongoUrl.Create(_settings.ConnectionString);
            var clientSettings = MongoClientSettings.FromUrl(mongoUrl);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(2);
            clientSettings.ConnectTimeout = TimeSpan.FromSeconds(2);

            var client = new MongoClient(clientSettings);
            var database = client.GetDatabase(_settings.DatabaseName);
            _auditEvents = database.GetCollection<AuditEvent>(_settings.CollectionName);
        }
    }

    [HttpGet]
    public async Task<ActionResult<AdminAuditResponse>> GetLatest()
    {
        if (!_settings.Enabled || _auditEvents == null)
        {
            var localEvents = await ReadLocalAuditEventsAsync();
            return Ok(new AdminAuditResponse(
                false,
                "MongoDB audit logging is disabled. Showing local App_Data/audit.log events.",
                localEvents));
        }

        try
        {
            var events = await _auditEvents
                .Find(_ => true)
                .SortByDescending(auditEvent => auditEvent.Time)
                .Limit(25)
                .ToListAsync();

            return Ok(new AdminAuditResponse(true, "MongoDB audit logging is enabled.", events));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "MongoDB audit logs are unavailable. Falling back to local audit log.");

            var localEvents = await ReadLocalAuditEventsAsync();
            return Ok(new AdminAuditResponse(
                false,
                $"MongoDB is unavailable. Showing local App_Data/audit.log events. {exception.Message}",
                localEvents));
        }
    }

    private async Task<IReadOnlyCollection<AuditEvent>> ReadLocalAuditEventsAsync()
    {
        var logPath = Path.Combine(_environment.ContentRootPath, "App_Data", "audit.log");

        if (!System.IO.File.Exists(logPath))
        {
            return Array.Empty<AuditEvent>();
        }

        var lines = await System.IO.File.ReadAllLinesAsync(logPath);

        return lines
            .Reverse()
            .Take(25)
            .Select(ParseAuditLine)
            .Where(auditEvent => auditEvent != null)
            .Cast<AuditEvent>()
            .ToList();
    }

    private static AuditEvent? ParseAuditLine(string line)
    {
        var parts = line.Split('\t', 4);
        if (parts.Length < 4 || !DateTimeOffset.TryParse(parts[0], out var time))
        {
            return null;
        }

        return new AuditEvent
        {
            Time = time,
            User = parts[1],
            Action = parts[2],
            Details = parts[3]
        };
    }
}

public sealed record AdminAuditResponse(
    bool Enabled,
    string Message,
    IReadOnlyCollection<AuditEvent> Events
);
