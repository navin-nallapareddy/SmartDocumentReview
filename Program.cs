using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SmartDocumentReview.Data;
using SmartDocumentReview.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load DATABASE_URL from environment (Render)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(databaseUrl))
    throw new InvalidOperationException("DATABASE_URL not configured.");

// Convert DATABASE_URL to Npgsql format
string ConvertDatabaseUrlToNpgsql(string dbUrl)
{
    var uri = new Uri(dbUrl);
    var userInfo = uri.UserInfo.Split(':');

    int port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port;

    return $"Host={uri.Host};Port={port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=true";
}

var connectionString = ConvertDatabaseUrlToNpgsql(databaseUrl);

// Register PostgreSQL DbContext
builder.Services.AddDbContext<TagDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PdfKeywordTagger>();
builder.Services.AddScoped<ResultStateService>();

// Setup Razor/Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Ensure the database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TagDbContext>();
    db.Database.EnsureCreated();
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
