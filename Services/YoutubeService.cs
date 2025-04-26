using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Mazzika.Models;
using Mazzika.Services;

namespace Mazzika.Services
{
    public class YouTubeService
    {
        private readonly Google.Apis.YouTube.v3.YouTubeService _youTubeService;

        public YouTubeService(string apiKey)
        {
            _youTubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = "Mazzika"
            });
        }

        public async Task<List<Video>> GetPopularVideosAsync()
        {
            var videosRequest = _youTubeService.Videos.List("snippet,contentDetails,statistics");
            videosRequest.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            videosRequest.MaxResults = 10;

            var videosResponse = await videosRequest.ExecuteAsync();
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
    }
}