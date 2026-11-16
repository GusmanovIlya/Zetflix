using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zetflix.Data;
using Zetflix.Models;
using Zetflix.Services;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ISiteSettingsService _settingsService;
    private readonly IAuditLogService _auditLogService;

    public AuthController(
        ApplicationDbContext context,
        IPasswordService passwordService,
        ISiteSettingsService settingsService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _passwordService = passwordService;
        _settingsService = settingsService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .Include(currentUser => currentUser.Role)
            .FirstOrDefaultAsync(currentUser => currentUser.Email == model.Email);

        if (user is null || !_passwordService.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        if (!_passwordService.IsHashed(user.PasswordHash))
        {
            user.PasswordHash = _passwordService.HashPassword(model.Password);
            await _context.SaveChangesAsync();
        }

        await SignInUser(user);
        await _auditLogService.LogAsync("auth.login", user.UserName, $"User {user.Email} signed in.");

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Profile");
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var settings = await _settingsService.GetAsync();

        if (!settings.AllowRegistration || settings.MaintenanceMode)
        {
            return RedirectToAction("Login");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var settings = await _settingsService.GetAsync();

        if (!settings.AllowRegistration || settings.MaintenanceMode)
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var emailExists = await _context.Users.AnyAsync(user => user.Email == model.Email);

        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "Пользователь с таким email уже существует.");
            return View(model);
        }

        var userRole = await _context.Roles.FirstAsync(role => role.Name == "User");
        var user = new User
        {
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim(),
            PasswordHash = _passwordService.HashPassword(model.Password),
            RoleId = userRole.Id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.Role = userRole;
        await SignInUser(user);
        await _auditLogService.LogAsync("auth.register", user.UserName, $"User {user.Email} registered.");

        return RedirectToAction("Index", "Profile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _auditLogService.LogAsync("auth.logout", User.Identity?.Name, "User signed out.");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.Name ?? "User")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
