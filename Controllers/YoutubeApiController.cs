using Microsoft.AspNetCore.Mvc;
using Mazzika.Services;  // Fixed namespace
using Mazzika.Models;

namespace Mazzika.Controllers
{
    public class YouTubeApiController : Controller
    {
        private readonly YouTubeService _youTubeService;
        
        public YouTubeApiController(YouTubeService youTubeService)
        {
            _youTubeService = youTubeService;
        }
    }
}