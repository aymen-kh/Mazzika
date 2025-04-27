using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Mazzika.Models;
using Mazzika.Services;
using Microsoft.Extensions.Logging;

namespace Mazzika.Services
{
    public class YouTubeService : IYouTubeService
    {
        private readonly Google.Apis.YouTube.v3.YouTubeService _youTubeService;
        private readonly ILogger<YouTubeService> _logger;

        public YouTubeService(string apiKey, ILogger<YouTubeService> logger)
        {
            _youTubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = "Mazzika"
            });
            _logger = logger;
        }

        public async Task<List<Video>> GetPopularVideosAsync()
        {
            try
            {
                _logger.LogInformation("Fetching popular videos.");

                var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
                videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                videosRequest.MaxResults = 10;

                _logger.LogInformation("Making API request with parameters: Chart={Chart}, RegionCode={RegionCode}, VideoCategoryId={VideoCategoryId}, MaxResults={MaxResults}", videosRequest.Chart, videosRequest.RegionCode, videosRequest.VideoCategoryId, videosRequest.MaxResults);

                var videosResponse = await videosRequest.ExecuteAsync();

                if (videosResponse == null)
                {
                    _logger.LogError("API response is null. Check if the API key is valid and has sufficient quota.");
                    return new List<Video>();
                }

                if (videosResponse.Items == null || !videosResponse.Items.Any())
                {
                    _logger.LogWarning("No videos returned from the API.");
                    return new List<Video>();
                }

                _logger.LogInformation("API returned {Count} videos.", videosResponse.Items.Count);

                var videos = videosResponse.Items.Select(item => new Video
                {
                    Id = item.Id,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                    PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    ChannelTitle = item.Snippet.ChannelTitle
                }).ToList();

                _logger.LogInformation("Fetched {Count} popular videos.", videos.Count);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching popular videos.");
                return new List<Video>();
            }
        }

        public async Task<List<Video>> SearchVideosAsync(string query)
        {
            var searchRequest = _youTubeService.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.MaxResults = 10;

            var searchResponse = await searchRequest.ExecuteAsync();
            return searchResponse.Items.Select(item => new Video
            {
                Id = item.Id.VideoId,
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                ChannelTitle = item.Snippet.ChannelTitle
            }).ToList();
        }

        public List<Video> GetTrendingVideos()
        {
            var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
            videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            videosRequest.MaxResults = 10;

            var videosResponse = videosRequest.Execute();

            if (videosResponse.Items == null || !videosResponse.Items.Any())
            {
                _logger.LogWarning("No videos returned from the API.");
                return new List<Video>();
            }

            return videosResponse.Items.Select(item => new Video
            {
                Id = item.Id,
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                ChannelTitle = item.Snippet.ChannelTitle
            }).ToList();
        }

        public async Task<List<Video>> GetTrendingVideosAsync(string regionCode = "US", int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Fetching trending videos for region: {RegionCode}", regionCode);

                var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
                videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                videosRequest.RegionCode = regionCode;
                videosRequest.MaxResults = maxResults;

                _logger.LogInformation("Making API request with parameters: Chart={Chart}, RegionCode={RegionCode}, VideoCategoryId={VideoCategoryId}, MaxResults={MaxResults}", videosRequest.Chart, videosRequest.RegionCode, videosRequest.VideoCategoryId, videosRequest.MaxResults);

                var videosResponse = await videosRequest.ExecuteAsync();

                if (videosResponse == null)
                {
                    _logger.LogError("API response is null. Check if the API key is valid and has sufficient quota.");
                    return new List<Video>();
                }

                if (videosResponse.Items == null || !videosResponse.Items.Any())
                {
                    _logger.LogWarning("No videos returned from the API.");
                    return new List<Video>();
                }

                _logger.LogInformation("API returned {Count} videos.", videosResponse.Items.Count);

                var videos = videosResponse.Items.Select(item => new Video
                {
                    Id = item.Id,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                    PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    ChannelTitle = item.Snippet.ChannelTitle
                }).ToList();

                _logger.LogInformation("Fetched {Count} trending videos for region: {RegionCode}", videos.Count, regionCode);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending videos for region: {RegionCode}", regionCode);
                return new List<Video>();
            }
        }

        public async Task<List<Video>> GetTrendingMusicVideosAsync(string regionCode = "US", int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Fetching trending music videos for region: {RegionCode}", regionCode);

                var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
                videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                videosRequest.RegionCode = regionCode;
                videosRequest.VideoCategoryId = "10"; // Category ID for Music
                videosRequest.MaxResults = maxResults;

                _logger.LogInformation("Making API request with parameters: Chart={Chart}, RegionCode={RegionCode}, VideoCategoryId={VideoCategoryId}, MaxResults={MaxResults}", videosRequest.Chart, videosRequest.RegionCode, videosRequest.VideoCategoryId, videosRequest.MaxResults);

                var videosResponse = await videosRequest.ExecuteAsync();

                if (videosResponse == null)
                {
                    _logger.LogError("API response is null. Check if the API key is valid and has sufficient quota.");
                    return new List<Video>();
                }

                if (videosResponse.Items == null || !videosResponse.Items.Any())
                {
                    _logger.LogWarning("No videos returned from the API.");
                    return new List<Video>();
                }

                _logger.LogInformation("API returned {Count} videos.", videosResponse.Items.Count);

                var videos = videosResponse.Items.Select(item => new Video
                {
                    Id = item.Id,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                    PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    ChannelTitle = item.Snippet.ChannelTitle
                }).ToList();

                _logger.LogInformation("Fetched {Count} trending music videos for region: {RegionCode}", videos.Count, regionCode);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending music videos for region: {RegionCode}", regionCode);
                return new List<Video>();
            }
        }

        public async Task<List<Video>> GetPersonalizedTrendingMusicAsync(HttpContext httpContext, int maxResults = 10)
        {
            try
            {
                // Determine the user's region from the Accept-Language header
                var userRegion = httpContext.Request.Headers["Accept-Language"].ToString().Split(',').FirstOrDefault()?.Split('-').LastOrDefault() ?? "US";

                _logger.LogInformation("Fetching personalized trending music for region: {UserRegion}", userRegion);

                if (string.IsNullOrEmpty(userRegion))
                {
                    _logger.LogWarning("User region is null or empty. Defaulting to 'US'.");
                    userRegion = "US";
                }

                var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
                videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                videosRequest.RegionCode = userRegion;
                videosRequest.VideoCategoryId = "10"; // Music category
                videosRequest.MaxResults = maxResults;

                _logger.LogInformation("Requesting trending music videos with the following parameters: RegionCode={RegionCode}, MaxResults={MaxResults}", userRegion, maxResults);

                _logger.LogInformation("Making API request with parameters: Chart={Chart}, RegionCode={RegionCode}, VideoCategoryId={VideoCategoryId}, MaxResults={MaxResults}", videosRequest.Chart, videosRequest.RegionCode, videosRequest.VideoCategoryId, videosRequest.MaxResults);

                try
                {
                    _logger.LogInformation("API request made with RegionCode={RegionCode}, MaxResults={MaxResults}", userRegion, maxResults);

                    var videosResponse = await videosRequest.ExecuteAsync();

                    if (videosResponse == null)
                    {
                        _logger.LogError("API response is null. Check if the API key is valid and has sufficient quota.");
                        return new List<Video>();
                    }

                    if (videosResponse.Items == null || !videosResponse.Items.Any())
                    {
                        _logger.LogWarning("No videos returned from the API.");
                        return new List<Video>();
                    }

                    _logger.LogInformation("API returned {Count} videos for RegionCode={RegionCode}", videosResponse.Items.Count, userRegion);

                    return videosResponse.Items.Select(item => new Video
                    {
                        Id = item.Id,
                        Title = item.Snippet.Title,
                        Description = item.Snippet.Description,
                        ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                        PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                        ChannelTitle = item.Snippet.ChannelTitle
                    }).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching trending music videos for region: {RegionCode}", userRegion);
                    return new List<Video>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching personalized trending music for region.");
                return new List<Video>();
            }
        }

        public async Task<List<Video>> GetPersonalizedTrendingMusicAsync(string regionCode = "US", int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Fetching personalized trending music for region: {RegionCode}", regionCode);

                var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
                videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
                videosRequest.RegionCode = regionCode;
                videosRequest.VideoCategoryId = "10"; // Music category
                videosRequest.MaxResults = maxResults;

                _logger.LogInformation("Making API request with parameters: Chart={Chart}, RegionCode={RegionCode}, VideoCategoryId={VideoCategoryId}, MaxResults={MaxResults}", videosRequest.Chart, videosRequest.RegionCode, videosRequest.VideoCategoryId, videosRequest.MaxResults);

                var videosResponse = await videosRequest.ExecuteAsync();

                if (videosResponse == null)
                {
                    _logger.LogError("API response is null. Check if the API key is valid and has sufficient quota.");
                    return new List<Video>();
                }

                if (videosResponse.Items == null || !videosResponse.Items.Any())
                {
                    _logger.LogWarning("No videos returned from the API.");
                    return new List<Video>();
                }

                _logger.LogInformation("API returned {Count} videos.", videosResponse.Items.Count);

                var videos = videosResponse.Items.Select(item => new Video
                {
                    Id = item.Id,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                    PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    ChannelTitle = item.Snippet.ChannelTitle
                }).ToList();

                _logger.LogInformation("Fetched {Count} personalized trending music videos for region: {RegionCode}", videos.Count, regionCode);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching personalized trending music for region: {RegionCode}", regionCode);
                return new List<Video>();
            }
        }

        public List<Video> GetRecommendedVideos()
        {
            // Placeholder logic for recommended videos
            // Replace this with actual logic if needed
            return new List<Video>
            {
                new Video
                {
                    Id = "sample1",
                    Title = "Recommended Video 1",
                    Description = "Description for Recommended Video 1",
                    ThumbnailUrl = "https://via.placeholder.com/150",
                    PublishedAt = DateTime.Now,
                    ChannelTitle = "Channel 1",
                    PlayCount = 0
                },
                new Video
                {
                    Id = "sample2",
                    Title = "Recommended Video 2",
                    Description = "Description for Recommended Video 2",
                    ThumbnailUrl = "https://via.placeholder.com/150",
                    PublishedAt = DateTime.Now,
                    ChannelTitle = "Channel 2",
                    PlayCount = 0
                }
            };
        }

        public async Task<List<Video>> GetUserChannelVideosAsync(string username)
        {
            try
            {
                _logger.LogInformation("Fetching channel videos for user: {Username}", username);

                var searchRequest = _youTubeService.Search.List("snippet");
                searchRequest.Q = username;
                searchRequest.Type = "channel";
                searchRequest.MaxResults = 1;

                var searchResponse = await searchRequest.ExecuteAsync();
                var channel = searchResponse.Items.FirstOrDefault();

                if (channel == null)
                {
                    _logger.LogWarning("No channel found for username: {Username}", username);
                    return new List<Video>();
                }

                var channelId = channel.Id.ChannelId;
                var playlistRequest = _youTubeService.Playlists.List("snippet");
                playlistRequest.ChannelId = channelId;
                playlistRequest.MaxResults = 1;

                var playlistResponse = await playlistRequest.ExecuteAsync();
                var uploadsPlaylist = playlistResponse.Items.FirstOrDefault();

                if (uploadsPlaylist == null)
                {
                    _logger.LogWarning("No uploads playlist found for channel: {ChannelId}. Fetching videos from subscriptions instead.", channelId);

                    var subscriptionsRequest = _youTubeService.Subscriptions.List("snippet,contentDetails");
                    subscriptionsRequest.Mine = true;
                    subscriptionsRequest.MaxResults = 10;

                    var subscriptionsResponse = await subscriptionsRequest.ExecuteAsync();

                    return subscriptionsResponse.Items.Select(sub => new Video
                    {
                        Id = sub.Snippet.ResourceId.ChannelId,
                        Title = sub.Snippet.Title,
                        Description = sub.Snippet.Description,
                        ThumbnailUrl = sub.Snippet.Thumbnails.High?.Url ?? sub.Snippet.Thumbnails.Default__.Url,
                        PublishedAt = sub.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    //    ChannelTitle = sub.Snippet.ChannelTitle
                    }).ToList();
                }

                var playlistId = uploadsPlaylist.Id;
                var playlistItemsRequest = _youTubeService.PlaylistItems.List("snippet");
                playlistItemsRequest.PlaylistId = playlistId;
                playlistItemsRequest.MaxResults = 10;

                var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();

                if (playlistItemsResponse.Items == null || !playlistItemsResponse.Items.Any())
                {
                    _logger.LogWarning("No videos returned from the API.");
                    return new List<Video>();
                }

                var videos = playlistItemsResponse.Items.Select(item => new Video
                {
                    Id = item.Snippet.ResourceId.VideoId,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                    PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    ChannelTitle = item.Snippet.ChannelTitle
                }).ToList();

                _logger.LogInformation("Fetched {Count} videos for user: {Username}", videos.Count, username);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user channel videos for username: {Username}", username);
                return new List<Video>();
            }
        }

        public async Task<List<Video>> GetUserTrendingVideosAsync(string username)
        {
            try
            {
                var searchRequest = _youTubeService.Search.List("snippet");
                searchRequest.Q = username;
                searchRequest.Type = "video";
                searchRequest.MaxResults = 10;

                var searchResponse = await searchRequest.ExecuteAsync();

                if (searchResponse.Items == null || !searchResponse.Items.Any())
                {
                    _logger.LogWarning("No videos returned from the API.");
                    return new List<Video>();
                }

                return searchResponse.Items.Select(item => new Video
                {
                    Id = item.Id.VideoId,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                    PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                    ChannelTitle = item.Snippet.ChannelTitle
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trending videos for user: {Username}", username);
                return new List<Video>();
            }
        }

        public async Task<List<Video>> GetHomePageVideosAsync(string accessToken, int maxResults = 10)
        {
            try
            {
                _logger.LogInformation("Fetching home page videos for the logged-in user.");

                // Create a new YouTube service instance with the access token
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);
                var youTubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mazzika"
                });

                // Get user's subscriptions
                var subscriptionsRequest = youTubeService.Subscriptions.List("snippet,contentDetails");
                subscriptionsRequest.Mine = true;
                subscriptionsRequest.MaxResults = maxResults;

                var subscriptionsResponse = await subscriptionsRequest.ExecuteAsync();
                var videos = new List<Video>();

                if (subscriptionsResponse?.Items != null)
                {
                    foreach (var subscription in subscriptionsResponse.Items)
                    {
                        var channelId = subscription.Snippet.ResourceId.ChannelId;
                        
                        // Get channel's uploads playlist
                        var channelsRequest = youTubeService.Channels.List("contentDetails");
                        channelsRequest.Id = channelId;
                        var channelsResponse = await channelsRequest.ExecuteAsync();
                        
                        var channel = channelsResponse.Items?.FirstOrDefault();
                        if (channel?.ContentDetails?.RelatedPlaylists?.Uploads == null) continue;

                        var uploadsPlaylistId = channel.ContentDetails.RelatedPlaylists.Uploads;

                        // Get videos from uploads playlist
                        var playlistItemsRequest = youTubeService.PlaylistItems.List("snippet");
                        playlistItemsRequest.PlaylistId = uploadsPlaylistId;
                        playlistItemsRequest.MaxResults = 5; // Limit videos per channel

                        var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();

                        if (playlistItemsResponse?.Items != null)
                        {
                            videos.AddRange(playlistItemsResponse.Items.Select(item => new Video
                            {
                                Id = item.Snippet.ResourceId.VideoId,
                                Title = item.Snippet.Title,
                                Description = item.Snippet.Description,
                                ThumbnailUrl = item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Default__.Url,
                                PublishedAt = item.Snippet.PublishedAtDateTimeOffset?.DateTime ?? DateTime.Now,
                                ChannelTitle = item.Snippet.ChannelTitle
                            }));
                        }
                    }
                }

                // Sort videos by publish date and take the most recent ones
                videos = videos.OrderByDescending(v => v.PublishedAt).Take(maxResults).ToList();
                
                _logger.LogInformation("Fetched {Count} home page videos for the logged-in user.", videos.Count);
                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching home page videos for the logged-in user.");
                return new List<Video>();
            }
        }
    }
}