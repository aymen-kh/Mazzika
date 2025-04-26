using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mazzika.Models;
using System.Linq;

namespace Mazzika.Controllers
{
    public class MusicDbContext : DbContext
    {
        public DbSet<TopTrack> TopTracks { get; set; }

        public MusicDbContext(DbContextOptions<MusicDbContext> options) : base(options) { }
    }

    public class MusicController : Controller
    {
        private readonly ILogger<MusicController> _logger;
        private readonly MusicDbContext _dbContext;

        public MusicController(ILogger<MusicController> logger, MusicDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost]
        public IActionResult AddToTopTracks([FromBody] Video video)
        {
            _logger.LogInformation("AddToTopTracks called with video ID: {VideoId}", video.Id);

            if (video == null || string.IsNullOrEmpty(video.Id))
            {
                _logger.LogError("Invalid video data received.");
                return BadRequest("Invalid video data.");
            }

            var existingTrack = _dbContext.TopTracks.FirstOrDefault(t => t.VideoId == video.Id);

            if (existingTrack != null)
            {
                existingTrack.PlayCount++;
                _logger.LogInformation("Updated play count for video ID: {VideoId}", video.Id);
            }
            else
            {
                var newTrack = new TopTrack
                {
                    VideoId = video.Id,
                    Title = video.Title,
                    Description = video.Description,
                    ThumbnailUrl = video.ThumbnailUrl,
                    PublishedAt = video.PublishedAt,
                    ChannelTitle = video.ChannelTitle,
                    PlayCount = 1
                };
                _dbContext.TopTracks.Add(newTrack);
                _logger.LogInformation("Added new track for video ID: {VideoId}", video.Id);
            }

            _dbContext.SaveChanges();
            _logger.LogInformation("Database updated successfully.");

            return Ok();
        }

        [HttpGet]
        public IActionResult TopTracks()
        {
            var topTracks = _dbContext.TopTracks.OrderByDescending(t => t.PlayCount).ToList();
            return View(topTracks);
        }

        [HttpGet]
        public IActionResult DebugTopTracks()
        {
            var topTracks = _dbContext.TopTracks.ToList();
            if (!topTracks.Any())
            {
                _logger.LogWarning("No tracks found in the database.");
                return Content("No tracks found.");
            }

            _logger.LogInformation("Retrieved {Count} tracks from the database.", topTracks.Count);
            return Json(topTracks);
        }
    }
}