// SmartDocumentReview/Program.cs
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Adjust these namespaces to match your project structure
using SmartDocumentReview.Data;      // TagDbContext
using SmartDocumentReview.Services;  // AuthService, ResultStateService (if you have them)

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// Services
// ------------------------------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// App services (register your own)
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ResultStateService>();

// DbContext: prefer DATABASE_URL if present, else appsettings.json "DefaultConnection"
string? connStr = GetPostgresConnectionStringFromEnv(builder.Configuration)
                  ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "No database connection string found. Set DATABASE_URL or add ConnectionStrings:DefaultConnection.");
}

builder.Services.AddDbContext<TagDbContext>(options =>
{
    options.UseNpgsql(connStr);
});

var app = builder.Build();

// ------------------------------
// Middleware
// ------------------------------

// If you host the app under a sub-path (e.g., /app), set ASPNETCORE_PATHBASE=/app
var pathBase = Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// Dev vs Prod exception handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Static files (ensure this is BEFORE routing/endpoints)
app.UseStaticFiles(new StaticFileOptions
{
    // Make sure PDFs are served with the correct content-type
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".pdf"] = "application/pdf"
        }
    }
});

// Optional: simple CSP that allows pdf.js web worker & blob images
app.Use(async (context, next) =>
{
    // Only set if not already set by a reverse proxy
    const string csp =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " + // viewer uses inline styles
        "img-src 'self' data: blob:; " +
        "worker-src 'self' blob:; " +
        "connect-src 'self'; " +
        "font-src 'self';";

    if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
        context.Response.Headers["Content-Security-Policy"] = csp;

    await next();
});

app.UseRouting();

// (Add auth here if you wire up real authentication)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

// ------------------------------
// Helpers
// ------------------------------
static string? GetPostgresConnectionStringFromEnv(IConfiguration config)
{
    // Accept either DATABASE_URL or a conventional "DefaultConnection" in appsettings.json
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(databaseUrl))
        return null;

    // Expected format:
    // postgres://username:password@host:port/database?sslmode=Require
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

        var host = uri.Host;
        var port = uri.Port <= 0 ? 5432 : uri.Port;
        var database = uri.AbsolutePath.Trim('/');

        // Preserve query string (e.g., sslmode)
        var query = uri.Query?.TrimStart('?');
        var extras = string.IsNullOrEmpty(query) ? "" : $"?{query}";

        return $"Host={host};Port={port};Username={username};Password={password};Database={database};{(string.IsNullOrEmpty(query) ? "" : query.Replace("&", ";"))}";
    }
    catch
    {
        // Fallback: just return as-is if parsing fails (Npgsql can sometimes accept it)
        return databaseUrl;
    }
}
