using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Zetflix.Data;
using Zetflix.Hubs;
using Zetflix.Middleware;
using Zetflix.Models;
using Zetflix.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddLocalization();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "image/svg+xml"
    });
});
builder.Services.Configure<MongoAuditSettings>(builder.Configuration.GetSection("MongoAudit"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IZetflixLocalizer, ZetflixLocalizer>();
builder.Services.AddSingleton<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IAdminAuthService, AdminAuthService>();
builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Error/403";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ModeratorOrAdmin", policy => policy.RequireRole("Admin", "Moderator"));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(() => CatalogSeeder.SeedAsync(app.Services));
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("ru")
    .AddSupportedCultures("ru", "en")
    .AddSupportedUICultures("ru", "en"));
app.UseRouting();
app.UseCors("DashboardDev");
app.UseMiddleware<AdminApiAuthMiddleware>();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseAuthentication();
app.UseMiddleware<RoleRefreshMiddleware>();
app.UseAuthorization();

app.UseStaticFiles();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run();
