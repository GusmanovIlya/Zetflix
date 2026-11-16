namespace Zetflix.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string? userName, string details);
}
