using Mazzika.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using Mazzika.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mazzika.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure YouTube API service
var apiKey = builder.Configuration["YouTube:ApiKey"] 
    ?? throw new InvalidOperationException("YouTube API key not configured");
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<YouTubeService>();
builder.Services.AddScoped<IYouTubeService>(provider => new YouTubeService(apiKey, provider.GetRequiredService<ILogger<YouTubeService>>()));

// Configure Cookie Policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"] 
        ?? throw new InvalidOperationException("Google ClientId not configured");
    options.ClientSecret = builder.Configuration["Google:ClientSecret"] 
        ?? throw new InvalidOperationException("Google ClientSecret not configured");
    options.SaveTokens = true;
    options.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = context =>
        {
            if (context.AccessToken != null)
            {
                context.Identity?.AddClaim(new System.Security.Claims.Claim("access_token", context.AccessToken));
            }
            if (context.User.TryGetProperty("picture", out var pictureElement))
            {
                var picture = pictureElement.GetString();
                if (!string.IsNullOrEmpty(picture))
                {
                    context.Identity?.AddClaim(new System.Security.Claims.Claim("urn:google:picture", picture));
                }
            }
            return Task.CompletedTask;
        },
        OnTicketReceived = context =>
        {
            context.ReturnUri = "/Music/Trending";
            return Task.CompletedTask;
        }
    };
});

// Configure SQLite database
builder.Services.AddDbContext<Mazzika.Data.MusicDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=music.db"));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<Mazzika.Data.MusicDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ensure that authentication is used before authorization
app.UseAuthentication();
app.UseAuthorization();

// Map default routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
