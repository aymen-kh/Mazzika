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
}

@section Scripts {
<script>
    function addToTopTracks(video) {
        fetch('/Music/AddToTopTracks', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(video)
        }).then(response => {
            if (response.ok) {
                console.log('Video added to top tracks.');
            } else {
                console.error('Failed to add video to top tracks.');
            }
        });
    }

    function onVideoPlayed(videoId, title, description, thumbnailUrl, publishedAt, channelTitle) {
        const video = {
            id: videoId,
            title: title,
            description: description,
            thumbnailUrl: thumbnailUrl,
            publishedAt: publishedAt,
            channelTitle: channelTitle
        };

        addToTopTracks(video);
    }
</script>
}