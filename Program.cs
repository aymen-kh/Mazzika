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
builder.Services.AddSingleton<YouTubeService>(new YouTubeService(apiKey));

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"] 
        ?? throw new InvalidOperationException("Google ClientId not configured");
    options.ClientSecret = builder.Configuration["Google:ClientSecret"] 
        ?? throw new InvalidOperationException("Google ClientSecret not configured");
    options.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");

    // Add custom claims using OnCreatingTicket
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = context =>
        {
            if (context.User.TryGetProperty("picture", out var pictureElement))
            {
                var picture = pictureElement.GetString();
                if (!string.IsNullOrEmpty(picture))
                {
                    context.Identity.AddClaim(new System.Security.Claims.Claim("urn:google:picture", picture));
                }
            }
            return Task.CompletedTask;
        }
    };
});

// Configure in-memory database
builder.Services.AddDbContext<MusicDbContext>(options =>
    options.UseInMemoryDatabase("MusicDb"));

var app = builder.Build();

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
