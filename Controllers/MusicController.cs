using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mazzika.Models;
using System.Text.Json;

namespace Mazzika.Controllers
{
    public class MusicController : Controller
    {
        public IActionResult Trending()
        {
            return View();
        }

        public IActionResult Recommended()
        {
            return View();
        }

        public IActionResult TopTracks()
        {
            var topTracksCookie = Request.Cookies["TopTracks"];
            List<Video> topTracks = new();

            if (!string.IsNullOrEmpty(topTracksCookie))
            {
                try
                {
                    topTracks = JsonSerializer.Deserialize<List<Video>>(topTracksCookie) ?? new List<Video>();
                }
                catch
                {
                    // Handle invalid cookie data
                    topTracks = new List<Video>();
                }
            }

            return View(topTracks);
        }

        [HttpPost]
        public IActionResult AddToTopTracks(Video video)
        {
            // Retrieve the "TopTracks" cookie
            var topTracksCookie = Request.Cookies["TopTracks"];
            List<Video> topTracks = new();

            if (!string.IsNullOrEmpty(topTracksCookie))
            {
                // Deserialize the cookie value into a list of videos
                topTracks = JsonSerializer.Deserialize<List<Video>>(topTracksCookie) ?? new List<Video>();
            }

            // Check if the video already exists in the list
            var existingVideo = topTracks.FirstOrDefault(v => v.Id == video.Id);
            if (existingVideo != null)
            {
                // Increment the play count
                existingVideo.PlayCount++;
            }
            else
            {
                // Add the new video with an initial play count of 1
                video.PlayCount = 1;
                topTracks.Add(video);
            }

            // Serialize the updated list and store it back in the cookie
            var options = new CookieOptions { Expires = DateTime.Now.AddDays(7) };
            Response.Cookies.Append("TopTracks", JsonSerializer.Serialize(topTracks), options);

            return Ok();
        }
    }
}