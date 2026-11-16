using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminUsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers()
    {
        var users = await _context.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .Include(user => user.Subscription)
            .OrderBy(user => user.UserName)
            .Select(user => new AdminUserDto(
                user.Id,
                user.UserName,
                user.Email,
                user.RoleId,
                user.Role != null ? user.Role.Name : "No role",
                user.SubscriptionId,
                user.Subscription != null ? user.Subscription.Name : "No subscription",
                "Active"
            ))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("options")]
    public async Task<ActionResult<AdminUserOptionsDto>> GetOptions()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new AdminOptionDto(role.Id, role.Name))
            .ToListAsync();

        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .OrderBy(subscription => subscription.Name)
            .Select(subscription => new AdminOptionDto(subscription.Id, subscription.Name))
            .ToListAsync();

        return Ok(new AdminUserOptionsDto(roles, subscriptions));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> UpdateUser(int id, UpdateAdminUserDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest("User name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }

        var roleExists = await _context.Roles.AnyAsync(role => role.Id == request.RoleId);

        if (!roleExists)
        {
            return BadRequest("Role was not found.");
        }

        if (request.SubscriptionId.HasValue)
        {
            var subscriptionExists = await _context.Subscriptions.AnyAsync(subscription => subscription.Id == request.SubscriptionId.Value);

            if (!subscriptionExists)
            {
                return BadRequest("Subscription was not found.");
            }
        }

        user.UserName = request.UserName.Trim();
        user.Email = request.Email.Trim();
        user.RoleId = request.RoleId;
        user.SubscriptionId = request.SubscriptionId;

        await _context.SaveChangesAsync();

        var updatedUser = await _context.Users
            .AsNoTracking()
            .Include(currentUser => currentUser.Role)
            .Include(currentUser => currentUser.Subscription)
            .Where(currentUser => currentUser.Id == id)
            .Select(currentUser => new AdminUserDto(
                currentUser.Id,
                currentUser.UserName,
                currentUser.Email,
                currentUser.RoleId,
                currentUser.Role != null ? currentUser.Role.Name : "No role",
                currentUser.SubscriptionId,
                currentUser.Subscription != null ? currentUser.Subscription.Name : "No subscription",
                "Active"
            ))
            .FirstAsync();

        return Ok(updatedUser);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .Include(currentUser => currentUser.Role)
            .FirstOrDefaultAsync(currentUser => currentUser.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        if (user.Role?.Name == "Admin")
        {
            var adminCount = await _context.Users
                .Include(currentUser => currentUser.Role)
                .CountAsync(currentUser => currentUser.Role != null && currentUser.Role.Name == "Admin");

            if (adminCount <= 1)
            {
                return BadRequest("The last admin account cannot be deleted.");
            }
        }

        var reviews = await _context.Reviews
            .Where(review => review.UserId == id)
            .ToListAsync();

        _context.Reviews.RemoveRange(reviews);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public sealed record AdminUserDto(
    int Id,
    string UserName,
    string Email,
    int RoleId,
    string Role,
    int? SubscriptionId,
    string Subscription,
    string Status
);

public sealed record AdminOptionDto(int Id, string Name);

public sealed record AdminUserOptionsDto(
    IReadOnlyCollection<AdminOptionDto> Roles,
    IReadOnlyCollection<AdminOptionDto> Subscriptions
);

public sealed record UpdateAdminUserDto(
    string UserName,
    string Email,
    int RoleId,
    int? SubscriptionId
);
