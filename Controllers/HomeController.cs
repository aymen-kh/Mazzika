using Microsoft.AspNetCore.Mvc;
using Mazzika.Services;
using Mazzika.Models;

namespace Mazzika.Controllers
{
    public class HomeController : Controller
    {
        private readonly YouTubeService _youTubeService;
        
        public HomeController(YouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }
        
      public async Task<IActionResult> Index(string searchQuery = null)
{
    List<Video> videos;

    if (string.IsNullOrEmpty(searchQuery))
    {
        videos = await _youTubeService.GetPopularVideosAsync();
    }
    else
    {
        videos = await _youTubeService.SearchVideosAsync(searchQuery);
    }

    // Shuffle the list randomly
    var random = new Random();
    var shuffledVideos = videos.OrderBy(_ => random.Next()).ToList();
    
    ViewBag.SearchQuery = searchQuery;
    
    return View(shuffledVideos);
}


    }
}