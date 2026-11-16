using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Services;

public interface IAdminAuthService
{
    Task<AdminSession?> LoginAsync(string email, string password);
    bool IsValidToken(string token);
    void Logout(string token);
}

public class AdminAuthService : IAdminAuthService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IPasswordService _passwordService;
    private readonly IDataProtector _protector;

    public AdminAuthService(
        IServiceScopeFactory scopeFactory,
        IPasswordService passwordService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _scopeFactory = scopeFactory;
        _passwordService = passwordService;
        _protector = dataProtectionProvider.CreateProtector("Zetflix.AdminSession.v1");
    }

    public async Task<AdminSession?> LoginAsync(string email, string password)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await context.Users
            .Include(currentUser => currentUser.Role)
            .FirstOrDefaultAsync(currentUser => currentUser.Email == email && currentUser.Role != null && currentUser.Role.Name == "Admin");

        if (user is null || !_passwordService.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        if (!_passwordService.IsHashed(user.PasswordHash))
        {
            user.PasswordHash = _passwordService.HashPassword(password);
            await context.SaveChangesAsync();
        }

        var session = new AdminSession(
            string.Empty,
            user.Id,
            user.UserName,
            user.Email,
            DateTime.UtcNow.AddHours(8)
        );

        var tokenPayload = JsonSerializer.Serialize(session);

        return session with { Token = _protector.Protect(tokenPayload) };
    }

    public bool IsValidToken(string token)
    {
        try
        {
            var json = _protector.Unprotect(token);
            var session = JsonSerializer.Deserialize<AdminSession>(json);

            return session?.ExpiresAtUtc > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    public void Logout(string token)
    {
        // The React dashboard removes the protected token on logout.
    }
}

public sealed record AdminSession(
    string Token,
    int UserId,
    string UserName,
    string Email,
    DateTime ExpiresAtUtc
);
