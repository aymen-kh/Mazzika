using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mazzika.Models;
using Mazzika.Services;
using Mazzika.Data;

namespace Mazzika.Controllers
{
    public class MusicController : Controller
    {
        private readonly ILogger<MusicController> _logger;
        private readonly MusicDbContext _dbContext;
        private readonly IYouTubeService _youTubeService;

        public MusicController(ILogger<MusicController> logger, MusicDbContext dbContext, IYouTubeService youTubeService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _youTubeService = youTubeService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToTopTracks([FromBody] Video video)
        {
            if (video == null)
            {
                _logger.LogError("Video object is null");
                return BadRequest("Video object cannot be null");
            }

            _logger.LogInformation("Received video data: ID={Id}, Title={Title}", video.Id, video.Title);

            if (string.IsNullOrEmpty(video.Id) || string.IsNullOrEmpty(video.Title))
            {
                _logger.LogError("Video data is incomplete. ID={Id}, Title={Title}", video.Id, video.Title);
                return BadRequest("Video data is incomplete");
            }

            try
            {
                var existingTrack = await _dbContext.TopTracks
                    .FirstOrDefaultAsync(t => t.VideoId == video.Id);

                if (existingTrack != null)
                {
                    existingTrack.PlayCount++;
                    existingTrack.LastPlayed = DateTime.UtcNow;
                    _dbContext.Entry(existingTrack).State = EntityState.Modified;
                }
                else
                {
                    var newTrack = new TopTrack
                    {
                        VideoId = video.Id,
                        Title = video.Title,
                        Description = video.Description ?? "",
                        ThumbnailUrl = video.ThumbnailUrl ?? "",
                        PublishedAt = video.PublishedAt,
                        ChannelTitle = video.ChannelTitle ?? "",
                        PlayCount = 1,
                        LastPlayed = DateTime.UtcNow,
                        HasBeenContinued = false
                    };
                    _logger.LogInformation("Creating new track entry: {Title}", newTrack.Title);
                    await _dbContext.TopTracks.AddAsync(newTrack);
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully updated play count for video ID: {VideoId}", video.Id);
                
                var updatedTracks = await _dbContext.TopTracks
                    .OrderByDescending(t => t.PlayCount)
                    .ThenByDescending(t => t.LastPlayed)
                    .ToListAsync();
                    
                return Ok(new { success = true, tracks = updatedTracks });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating top tracks for video ID: {VideoId}", video.Id);
                return StatusCode(500, new { success = false, message = "Error updating top tracks" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TopTracks()
        {
            try
            {
                // Fetch top tracks ordered by LastPlayed and PlayCount
                var topTracks = await _dbContext.TopTracks
                    .OrderByDescending(t => t.LastPlayed)
                    .ThenByDescending(t => t.PlayCount)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} top tracks", topTracks.Count);
                return View(topTracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top tracks");
                return View(new List<TopTrack>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugTopTracks()
        {
            try
            {
                var topTracks = await _dbContext.TopTracks.ToListAsync();
                if (!topTracks.Any())
                {
                    _logger.LogWarning("No tracks found in the database.");
                    return Content("No tracks found.");
                }

                _logger.LogInformation("Retrieved {Count} tracks from the database.", topTracks.Count);
                return Json(topTracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tracks for debug");
                return StatusCode(500, "Error retrieving tracks");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Trending()
        {
            List<Video> trendingVideos;
            _logger.LogInformation("User is authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

            try {
                // Get trending music videos
                var videos = await _youTubeService.SearchVideosAsync("arabic music trending");
                trendingVideos = videos.Select(v => new Video
                {
                    Id = v.Id, // This should already be the correct video ID from YouTube
                    Title = v.Title,
                    Description = v.Description,
                    ThumbnailUrl = v.ThumbnailUrl,
                    PublishedAt = v.PublishedAt,
                    ChannelTitle = v.ChannelTitle
                }).ToList();

                _logger.LogInformation("Successfully fetched {Count} trending videos", trendingVideos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending videos");
                trendingVideos = new List<Video>();
            }

            return View(trendingVideos);
        }

        [HttpGet]
        public async Task<IActionResult> Recommended()
        {
            _logger.LogInformation("User is authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
            var allVideos = new List<Video>();

            try
            {
                // Get recent plays from database
                var recentPlays = _dbContext.TopTracks
                    .OrderByDescending(t => t.PlayCount)
                    .Take(6)
                    .Select(t => new Video
                    {
                        Id = t.VideoId, // Use VideoId from TopTracks
                        Title = t.Title,
                        Description = t.Description,
                        ThumbnailUrl = t.ThumbnailUrl,
                        PublishedAt = t.PublishedAt,
                        ChannelTitle = t.ChannelTitle
                    })
                    .ToList();
                allVideos.AddRange(recentPlays);

                // Get mix suggestions (Arabic music focused)
                var arabicMusicMix = await _youTubeService.SearchVideosAsync("arabic music mix");
                if (arabicMusicMix.Any())
                {
                    allVideos.AddRange(arabicMusicMix.Take(6));
                }

                _logger.LogInformation("Successfully fetched recommended videos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recommended videos");
                ViewBag.Message = "Unable to fetch recommendations at the moment. Please try again later.";
            }

            return View(allVideos);
        }

        [HttpGet]
        public async Task<IActionResult> ContinuePlaylist(string currentVideoId)
        {
            try
            {
                // Get the current track
                var currentTrack = await _dbContext.TopTracks
                    .FirstOrDefaultAsync(t => t.VideoId == currentVideoId);
                
                if (currentTrack == null)
                {
                    return NotFound("Current track not found");
                }

                // Find the next most played track that hasn't been continued from
                var nextTrack = await _dbContext.TopTracks
                    .Where(t => !t.HasBeenContinued && t.VideoId != currentVideoId)
                    .OrderByDescending(t => t.PlayCount)
                    .FirstOrDefaultAsync();

                if (nextTrack == null)
                {
                    // If all tracks have been continued, reset the flags and try again
                    await _dbContext.TopTracks
                        .ForEachAsync(t => t.HasBeenContinued = false);
                    await _dbContext.SaveChangesAsync();
                    
                    nextTrack = await _dbContext.TopTracks
                        .Where(t => t.VideoId != currentVideoId)
                        .OrderByDescending(t => t.PlayCount)
                        .FirstOrDefaultAsync();
                }

                if (nextTrack == null)
                {
                    return NotFound("No tracks available to continue to");
                }

                // Mark the track as continued
                nextTrack.HasBeenContinued = true;
                await _dbContext.SaveChangesAsync();

                return Ok(new { videoId = nextTrack.VideoId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next track to continue to");
                return StatusCode(500, "Error getting next track");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVideoDetails(string videoId)
        {
            try
            {
                var video = await _youTubeService.GetVideoDetailsAsync(videoId);
                if (video == null)
                {
                    return NotFound();
                }
                return Json(video);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video details for ID: {VideoId}", videoId);
                return StatusCode(500, new { error = "Failed to get video details" });
            }
        }

        private async Task<Video?> GetNextRecommendedVideo(string currentVideoId)
        {
            // First try to get a video from user's history
            var userHistory = await _dbContext.TopTracks
                .Where(t => t.VideoId != currentVideoId)
                .OrderByDescending(t => t.LastPlayed)
                .FirstOrDefaultAsync();

            if (userHistory != null)
            {
                return new Video
                {
                    Id = userHistory.VideoId,
                    Title = userHistory.Title,
                    Description = userHistory.Description,
                    ThumbnailUrl = userHistory.ThumbnailUrl,
                    PublishedAt = userHistory.PublishedAt,
                    ChannelTitle = userHistory.ChannelTitle
                };
            }

            // If no history, get a trending video
            var trendingVideos = await _youTubeService.SearchVideosAsync("arabic music trending");
            return trendingVideos.FirstOrDefault();
        }
    }
}