using Zetflix.Services;

namespace Zetflix.Middleware;

public class AdminApiAuthMiddleware
{
    private readonly RequestDelegate _next;

    public AdminApiAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAdminAuthService authService)
    {
        if (!context.Request.Path.StartsWithSegments("/api/admin") ||
            context.Request.Path.StartsWithSegments("/api/admin/auth/login") ||
            HttpMethods.IsOptions(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var authorization = context.Request.Headers.Authorization.ToString();
        var token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(token) || !authService.IsValidToken(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Admin authorization is required.");
            return;
        }

        await _next(context);
    }
}
