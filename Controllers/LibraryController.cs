using Microsoft.AspNetCore.Mvc;
using Mazzika.Services;
using Mazzika.Models;

namespace Mazzika.Controllers
{
    public class LibraryController : Controller
    {
        private readonly YouTubeService _youTubeService;

        public LibraryController(YouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }

        public IActionResult Index()
        {
            var trendingVideos = _youTubeService.GetTrendingVideos();
            return View(trendingVideos);
        }

        public IActionResult Trending()
        {
            var trendingVideos = _youTubeService.GetTrendingVideos();

            if (trendingVideos == null || !trendingVideos.Any())
            {
                ViewBag.Message = "No trending music available.";
            }

            return View(trendingVideos);
        }
    }
}