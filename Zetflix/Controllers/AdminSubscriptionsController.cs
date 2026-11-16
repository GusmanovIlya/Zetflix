using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;

namespace Zetflix.Controllers;

[ApiController]
[Route("api/admin/subscriptions")]
public class AdminSubscriptionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminSubscriptionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdminSubscriptionDto>>> GetSubscriptions()
    {
        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .OrderBy(subscription => subscription.Price)
            .Select(subscription => new AdminSubscriptionDto(
                subscription.Id,
                subscription.Name,
                subscription.Price,
                subscription.DurationInDays,
                subscription.Users.Count
            ))
            .ToListAsync();

        return Ok(subscriptions);
    }

    [HttpPost]
    public async Task<ActionResult<AdminSubscriptionDto>> CreateSubscription(UpdateAdminSubscriptionDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Subscription name is required.");
        }

        if (request.Price < 0)
        {
            return BadRequest("Price must be zero or greater.");
        }

        if (request.DurationInDays <= 0)
        {
            return BadRequest("Duration must be greater than zero.");
        }

        var subscription = new Subscription
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            DurationInDays = request.DurationInDays,
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubscriptions), new { id = subscription.Id }, new AdminSubscriptionDto(
            subscription.Id,
            subscription.Name,
            subscription.Price,
            subscription.DurationInDays,
            0
        ));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminSubscriptionDto>> UpdateSubscription(int id, UpdateAdminSubscriptionDto request)
    {
        var subscription = await _context.Subscriptions
            .Include(currentSubscription => currentSubscription.Users)
            .FirstOrDefaultAsync(currentSubscription => currentSubscription.Id == id);

        if (subscription is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Subscription name is required.");
        }

        if (request.Price < 0)
        {
            return BadRequest("Price must be zero or greater.");
        }

        if (request.DurationInDays <= 0)
        {
            return BadRequest("Duration must be greater than zero.");
        }

        subscription.Name = request.Name.Trim();
        subscription.Price = request.Price;
        subscription.DurationInDays = request.DurationInDays;

        await _context.SaveChangesAsync();

        return Ok(new AdminSubscriptionDto(
            subscription.Id,
            subscription.Name,
            subscription.Price,
            subscription.DurationInDays,
            subscription.Users.Count
        ));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _context.Subscriptions
            .Include(currentSubscription => currentSubscription.Users)
            .FirstOrDefaultAsync(currentSubscription => currentSubscription.Id == id);

        if (subscription is null)
        {
            return NotFound();
        }

        if (subscription.Users.Count > 0)
        {
            return Conflict("Subscription is assigned to users.");
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public sealed record AdminSubscriptionDto(
    int Id,
    string Name,
    decimal Price,
    int DurationInDays,
    int UserCount
);

public sealed record UpdateAdminSubscriptionDto(
    string Name,
    decimal Price,
    int DurationInDays
);
