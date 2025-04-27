using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mazzika.Models;
using System.Linq;
using System.Threading.Tasks;
using Mazzika.Services;

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
        private readonly IYouTubeService _youTubeService;

        public MusicController(ILogger<MusicController> logger, MusicDbContext dbContext, IYouTubeService youTubeService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _youTubeService = youTubeService;
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

        [HttpGet]
        public async Task<IActionResult> Trending()
        {
            List<Video> trendingVideos;
            _logger.LogInformation("User is authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

            if (User.Identity?.IsAuthenticated == true)
            {
                var accessToken = ((ClaimsPrincipal)User).FindFirstValue("access_token");
                _logger.LogInformation("Access token: {AccessToken}", accessToken ?? "null");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Get trending music videos using the OAuth token to get region-specific content
                    var youTubeService = new Google.Apis.YouTube.v3.YouTubeService(new Google.Apis.Services.BaseClientService.Initializer
                    {
                        HttpClientInitializer = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken),
                        ApplicationName = "Mazzika"
                    });

                    var videosRequest = youTubeService.Videos.List("snippet,contentDetails,statistics");
                    videosRequest.Chart = Google.Apis.YouTube.v3.VideosResource.ListRequest.ChartEnum.MostPopular;
                    videosRequest.VideoCategoryId = "10"; // Music category
                    videosRequest.MaxResults = 20;
                    videosRequest.RegionCode = "TN"; // Set to Tunisia by default since that's where we want to focus

                    var videosResponse = await videosRequest.ExecuteAsync();
                    
                    if (videosResponse?.Items != null)
                    {
                        trendingVideos = videosResponse.Items.Select(item => new Video
                        {
                            Id = item.Id,
                            Title = item.Snippet.Title,
                            Description = item.Snippet.Description,
                            ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                            PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                            ChannelTitle = item.Snippet.ChannelTitle
                        }).ToList();
                    }
                    else
                    {
                        _logger.LogWarning("No videos returned from authenticated request. Falling back to general trending.");
                        trendingVideos = await _youTubeService.GetTrendingMusicVideosAsync("TN");
                    }
                }
                else 
                {
                    _logger.LogWarning("Access token is missing for the logged-in user.");
                    // Fallback to regular trending videos if token is missing
                    trendingVideos = await _youTubeService.GetTrendingMusicVideosAsync("TN");
                }
            }
            else
            {
                _logger.LogInformation("Fetching general trending music for Tunisia");
                // Get regular trending music videos for Tunisia
                trendingVideos = await _youTubeService.GetTrendingMusicVideosAsync("TN");
            }

            if (!trendingVideos.Any())
            {
                ViewBag.Message = "No trending music available at the moment.";
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
                        Id = t.VideoId,
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

                // Get trending music in Tunisia
                var trendingMusic = await _youTubeService.GetTrendingMusicVideosAsync("TN", 6);
                if (trendingMusic.Any())
                {
                    allVideos.AddRange(trendingMusic);
                }

                if (User.Identity?.IsAuthenticated == true)
                {
                    var accessToken = ((ClaimsPrincipal)User).FindFirstValue("access_token");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        // Initialize YouTube service with OAuth token
                        var youTubeService = new Google.Apis.YouTube.v3.YouTubeService(new Google.Apis.Services.BaseClientService.Initializer
                        {
                            HttpClientInitializer = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken),
                            ApplicationName = "Mazzika"
                        });

                        // Get personalized recommendations
                        var recommendRequest = youTubeService.Activities.List("snippet,contentDetails");
                        recommendRequest.Mine = true;
                        recommendRequest.MaxResults = 6;

                        var response = await recommendRequest.ExecuteAsync();
                        if (response?.Items != null)
                        {
                            foreach (var item in response.Items)
                            {
                                string? videoId = null;
                                if (item.ContentDetails?.Upload != null)
                                {
                                    videoId = item.ContentDetails.Upload.VideoId;
                                }
                                else if (item.ContentDetails?.Like != null && item.ContentDetails.Like.ResourceId?.Kind == "youtube#video")
                                {
                                    videoId = item.ContentDetails.Like.ResourceId.VideoId;
                                }

                                if (!string.IsNullOrEmpty(videoId))
                                {
                                    var videoRequest = youTubeService.Videos.List("snippet");
                                    videoRequest.Id = videoId;
                                    var videoResponse = await videoRequest.ExecuteAsync();
                                    var videoItem = videoResponse.Items?.FirstOrDefault();

                                    if (videoItem != null)
                                    {
                                        allVideos.Add(new Video
                                        {
                                            Id = videoId,
                                            Title = videoItem.Snippet.Title,
                                            Description = videoItem.Snippet.Description,
                                            ThumbnailUrl = videoItem.Snippet.Thumbnails.High?.Url ?? videoItem.Snippet.Thumbnails.Default__.Url,
                                            PublishedAt = videoItem.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                                            ChannelTitle = videoItem.Snippet.ChannelTitle
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recommended videos");
                ViewBag.Message = "Unable to fetch recommendations at the moment. Please try again later.";
            }

            return View(allVideos);
        }
    }
}