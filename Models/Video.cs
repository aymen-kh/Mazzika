using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mazzika.Models
{
    public class Video
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string ChannelTitle { get; set; } = string.Empty;
        public int PlayCount { get; set; } // Property to track play count
    }

    [Table("TopTracks")]
    public class TopTrack
    {
        [Key]
        public int Id { get; set; }

        public string VideoId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ThumbnailUrl { get; set; }

        public DateTime PublishedAt { get; set; }

        public string ChannelTitle { get; set; }

        public int PlayCount { get; set; }
    }
}