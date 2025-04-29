using System;

namespace Mazzika.Models
{
    public class Video
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public string ChannelTitle { get; set; }
        public int PlayCount { get; set; }
        public DateTime LastPlayed { get; set; }
    }

    public class TopTrack
    {
        public int Id { get; set; }
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public string ChannelTitle { get; set; }
        public int PlayCount { get; set; }
        public DateTime LastPlayed { get; set; }
        public string FormattedLastPlayed => LastPlayed.ToString("MMM dd, yyyy HH:mm");
        public bool HasBeenContinued { get; set; }
    }
}