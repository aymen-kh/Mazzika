using Microsoft.AspNetCore.Mvc;
using Mazzika.Services;
using Mazzika.Models;

namespace Mazzika.Controllers
{
    public class HomeController : Controller
    {
        private readonly IYouTubeService _youTubeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IYouTubeService youTubeService, ILogger<HomeController> logger)
        {
            _youTubeService = youTubeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Video> homePageVideos;

            _logger.LogInformation("User is authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

            if (User.Identity?.IsAuthenticated == true)
            {
                // Retrieve the access token from the user's claims
                var accessToken = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;

                _logger.LogInformation("Access token: {AccessToken}", accessToken ?? "null");

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Access token is missing for the logged-in user.");
                    ViewBag.Message = "Unable to fetch personalized videos. Please log in again.";
                    return View(new List<Video>());
                }

                // Fetch home page videos for the logged-in user
                homePageVideos = await _youTubeService.GetHomePageVideosAsync(accessToken);
            }
            else
            {
                // Fetch popular videos for guests
                homePageVideos = await _youTubeService.GetPopularVideosAsync();
            }

            if (!homePageVideos.Any())
            {
                ViewBag.Message = "No videos available at the moment.";
            }

            return View(homePageVideos);
        }
    }
}