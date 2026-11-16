using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Middleware;

public class RoleRefreshMiddleware
{
    private readonly RequestDelegate _next;

    public RoleRefreshMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdValue, out var userId) && context.User.Identity is ClaimsIdentity identity)
            {
                var user = await dbContext.Users
                    .AsNoTracking()
                    .Include(currentUser => currentUser.Role)
                    .FirstOrDefaultAsync(currentUser => currentUser.Id == userId);

                if (user == null)
                {
                    await context.SignOutAsync();
                }
                else
                {
                    ReplaceClaim(identity, ClaimTypes.Name, user.UserName);
                    ReplaceClaim(identity, ClaimTypes.Email, user.Email);
                    ReplaceClaim(identity, ClaimTypes.Role, user.Role?.Name ?? "User");
                }
            }
        }

        await _next(context);
    }

    private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string value)
    {
        foreach (var claim in identity.FindAll(claimType).ToList())
        {
            identity.RemoveClaim(claim);
        }

        identity.AddClaim(new Claim(claimType, value));
    }
}
