using Mazzika.Models;

namespace Mazzika.Services
{
    public interface IYouTubeService
    {
        Task<List<Video>> GetPopularVideosAsync();
        Task<List<Video>> SearchVideosAsync(string query);
        List<Video> GetTrendingVideos();
        List<Video> GetRecommendedVideos();
        Task<List<Video>> GetUserChannelVideosAsync(string username);
        Task<List<Video>> GetUserTrendingVideosAsync(string username);
        Task<List<Video>> GetTrendingVideosAsync(string regionCode = "US", int maxResults = 10);
        Task<List<Video>> GetTrendingMusicVideosAsync(string regionCode = "US", int maxResults = 10);
        Task<List<Video>> GetPersonalizedTrendingMusicAsync(string userRegion, int maxResults = 10);
        Task<List<Video>> GetHomePageVideosAsync(string accessToken, int maxResults = 10);
    }
}