using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Mazzika.Models;

namespace Mazzika.Services
{
    public class YouTubeService
    {
        private readonly Google.Apis.YouTube.v3.YouTubeService _youtubeService;
        
        public YouTubeService(string apiKey)
        {
            _youtubeService = new Google.Apis.YouTube.v3.YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApiKey = apiKey,
                    ApplicationName = "Mazzika"
                });
        }
        
        public async Task<List<Video>> GetPopularVideosAsync(int maxResults = 10)
        {
            var request = _youtubeService.Videos.List("snippet");
            request.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            request.MaxResults = maxResults;
            request.RegionCode = "US";
            
            var response = await request.ExecuteAsync();
            
            return response.Items.Select(v => new Video
            {
                Id = v.Id,
                Title = v.Snippet.Title,
                Description = v.Snippet.Description,
                ThumbnailUrl = v.Snippet.Thumbnails.Medium?.Url ?? string.Empty,
                PublishedAt = v.Snippet.PublishedAt ?? DateTime.MinValue,
                ChannelTitle = v.Snippet.ChannelTitle
            }).ToList();
        }
        public async Task<List<Video>> SearchVideosAsync(string query, int maxResults = 10)
{
    var request = _youtubeService.Search.List("snippet");
    request.Q = query;
    request.Type = "video";
    request.MaxResults = maxResults;
    request.RegionCode = "US";

    var response = await request.ExecuteAsync();

    return response.Items.Select(item => new Video
    {
        Id = item.Id.VideoId,
        Title = item.Snippet.Title,
        Description = item.Snippet.Description,
        ThumbnailUrl = item.Snippet.Thumbnails.Medium?.Url ?? string.Empty,
        PublishedAt = item.Snippet.PublishedAt ?? DateTime.MinValue,
        ChannelTitle = item.Snippet.ChannelTitle
    }).ToList();
}

    }
}