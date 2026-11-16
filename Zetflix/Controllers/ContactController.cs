using System.Net;
using Microsoft.AspNetCore.Mvc;
using Zetflix.Services;
using Zetflix.ViewModels;

namespace Zetflix.Controllers;

public class ContactController : Controller
{
    private readonly ISiteSettingsService _settingsService;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLogService;

    public ContactController(
        ISiteSettingsService settingsService,
        IEmailService emailService,
        IAuditLogService auditLogService)
    {
        _settingsService = settingsService;
        _emailService = emailService;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new ContactViewModel
        {
            Settings = await _settingsService.GetAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model)
    {
        model.Settings = await _settingsService.GetAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Settings.MaintenanceMode)
        {
            ModelState.AddModelError(string.Empty, "Сайт находится в режиме обслуживания. Отправка сообщений временно недоступна.");
            return View(model);
        }

        var safeMessage = WebUtility.HtmlEncode(model.Message).Replace("\n", "<br>");
        var supportSubject = $"Zetflix contact: {model.Subject}";
        var supportBody = $"""
            <h2>New Zetflix contact message</h2>
            <p><strong>Name:</strong> {WebUtility.HtmlEncode(model.Name)}</p>
            <p><strong>Email:</strong> {WebUtility.HtmlEncode(model.Email)}</p>
            <p><strong>Subject:</strong> {WebUtility.HtmlEncode(model.Subject)}</p>
            <p>{safeMessage}</p>
            """;

        var sentToSupport = await _emailService.SendAsync(model.Settings.SupportEmail, supportSubject, supportBody);
        var sentToUser = await _emailService.SendAsync(
            model.Email,
            "Zetflix received your message",
            $"""
            <h2>Thanks, {WebUtility.HtmlEncode(model.Name)}!</h2>
            <p>We received your message and will reply to this email address.</p>
            <p>{safeMessage}</p>
            """);

        model.Sent = sentToSupport || sentToUser;
        await _auditLogService.LogAsync("contact.message", model.Email, $"Contact message submitted: {model.Subject}");

        ModelState.Clear();
        return View(model);
    }
}
