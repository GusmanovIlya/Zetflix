using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Zetflix.Hubs;
using Zetflix.Models;

namespace Zetflix.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuditLogService> _logger;
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly MongoAuditSettings _mongoSettings;
    private readonly IMongoCollection<AuditEvent>? _auditEvents;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public AuditLogService(
        IWebHostEnvironment environment,
        ILogger<AuditLogService> logger,
        IHubContext<NotificationsHub> hubContext,
        IOptions<MongoAuditSettings> mongoOptions)
    {
        _environment = environment;
        _logger = logger;
        _hubContext = hubContext;
        _mongoSettings = mongoOptions.Value;

        if (_mongoSettings.Enabled)
        {
            var mongoUrl = MongoUrl.Create(_mongoSettings.ConnectionString);
            var clientSettings = MongoClientSettings.FromUrl(mongoUrl);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(2);
            clientSettings.ConnectTimeout = TimeSpan.FromSeconds(2);

            var client = new MongoClient(clientSettings);
            var database = client.GetDatabase(_mongoSettings.DatabaseName);
            _auditEvents = database.GetCollection<AuditEvent>(_mongoSettings.CollectionName);
        }
    }

    public async Task LogAsync(string action, string? userName, string details)
    {
        var logDirectory = Path.Combine(_environment.ContentRootPath, "App_Data");
        var logPath = Path.Combine(logDirectory, "audit.log");
        var safeUser = string.IsNullOrWhiteSpace(userName) ? "anonymous" : userName;
        var eventTime = DateTimeOffset.UtcNow;
        var line = $"{eventTime:O}\t{safeUser}\t{action}\t{details}{Environment.NewLine}";
        var auditEvent = new AuditEvent
        {
            Time = eventTime,
            User = safeUser,
            Action = action,
            Details = details
        };

        try
        {
            Directory.CreateDirectory(logDirectory);
            await _lock.WaitAsync();
            await File.AppendAllTextAsync(logPath, line);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not write audit log entry for {Action}.", action);
        }
        finally
        {
            if (_lock.CurrentCount == 0)
            {
                _lock.Release();
            }
        }

        if (_auditEvents != null)
        {
            try
            {
                await _auditEvents.InsertOneAsync(auditEvent);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not write audit event to MongoDB for {Action}.", action);
            }
        }

        await _hubContext.Clients.All.SendAsync("ReceiveActivity", new
        {
            Time = eventTime,
            User = safeUser,
            Action = action,
            Details = details
        });
    }
}
