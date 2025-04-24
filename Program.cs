using Mazzika.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var apiKey = builder.Configuration["YouTube:ApiKey"] ?? 
    throw new InvalidOperationException("YouTube API key not configured");
builder.Services.AddSingleton<YouTubeService>(new YouTubeService(apiKey));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Google";
})
.AddCookie("Cookies")
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"] 
        ?? throw new InvalidOperationException("Missing ClientId");
    options.ClientSecret = builder.Configuration["Google:ClientSecret"] 
        ?? throw new InvalidOperationException("Missing ClientSecret");
    options.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");

    // Manually add the picture claim from the payload
    options.ClaimActions.Add(new Microsoft.AspNetCore.Authentication.OAuth.Claims.JsonKeyClaimAction(
        "urn:google:picture", "string", "picture"));
});

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
