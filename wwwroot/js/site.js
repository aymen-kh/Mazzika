// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function savePlayedTrack(trackId) {
    let tracks = JSON.parse(localStorage.getItem('guestTopTracks') || '[]');
    if (!tracks.includes(trackId)) {
        tracks.push(trackId);
        localStorage.setItem('guestTopTracks', JSON.stringify(tracks));
    }
}

function getPlayedTracks() {
    return JSON.parse(localStorage.getItem('guestTopTracks') || '[]');
}

// Global player state
let player;
let playlist = [];
let playlistTitles = [];
let playlistThumbs = [];
let currentIndex = 0;
let playPauseIcon;
let progressSlider;
let isSeeking = false;
let updateInterval;
let isPlayerReady = false;

// Load saved state from localStorage
function loadPlayerState() {
    try {
        const state = JSON.parse(localStorage.getItem('playerState'));
        if (state) {
            playlist = state.playlist || [];
            playlistTitles = state.playlistTitles || [];
            playlistThumbs = state.playlistThumbs || [];
            currentIndex = state.currentIndex || 0;
        }
    } catch (error) {
        console.error('Error loading player state:', error);
    }
}

// Save player state to localStorage
function savePlayerState() {
    try {
        const state = {
            playlist,
            playlistTitles,
            playlistThumbs,
            currentIndex
        };
        localStorage.setItem('playerState', JSON.stringify(state));
    } catch (error) {
        console.error('Error saving player state:', error);
    }
}

// YouTube IFrame API callback
function onYouTubeIframeAPIReady() {
    player = new YT.Player('player', {
        height: '0',
        width: '0',
        videoId: '',
        events: {
            'onReady': onPlayerReady,
            'onStateChange': onPlayerStateChange,
            'onError': onPlayerError
        }
    });
}

function onPlayerReady(event) {
    isPlayerReady = true;
    loadPlayerState();
    if (playlist.length > 0) {
        loadVideo(currentIndex);
    }
}

function onPlayerError(event) {
    console.error('YouTube player error:', event.data);
    // Skip to next song on error
    if (playlist.length > 1) {
        nextSong();
    }
}

function onPlayerStateChange(event) {
    if (event.data === YT.PlayerState.PLAYING) {
        startUpdatingProgress();
        if (playPauseIcon) {
            playPauseIcon.className = 'bi bi-pause-fill';
        }
    } else {
        if (playPauseIcon) {
            playPauseIcon.className = 'bi bi-play-fill';
        }
    }

    // Auto play next when current song ends
    if (event.data === YT.PlayerState.ENDED && playlist.length > 1) {
        nextSong();
    }
}

function playVideo(videoId, title, description, thumbnailUrl, publishedAt, channelTitle) {
    if (!videoId || !title) {
        console.error('Invalid video data:', { videoId, title });
        return;
    }

    // Save track to guest history
    savePlayedTrack(videoId);

    // Add to playlist if not already present
    const existingIndex = playlist.indexOf(videoId);
    if (existingIndex === -1) {
        playlist.push(videoId);
        playlistTitles.push(title);
        playlistThumbs.push(thumbnailUrl);
        currentIndex = playlist.length - 1;
    } else {
        currentIndex = existingIndex;
    }

    // Update UI
    const playerBar = document.getElementById('player-bar');
    const playerThumb = document.getElementById('player-thumbnail');
    const songTitle = document.getElementById('song-title');
    
    if (playerBar && playerThumb && songTitle) {
        playerBar.style.display = 'flex';
        playerThumb.src = thumbnailUrl;
        songTitle.innerText = title;
    }

    // Initialize player if needed
    if (!player || !isPlayerReady) {
        player = new YT.Player('player', {
            height: '0',
            width: '0',
            videoId: videoId,
            events: {
                'onReady': onPlayerReady,
                'onStateChange': onPlayerStateChange
            }
        });
        return;
    }

    // Play video and save state
    player.loadVideoById(videoId);
    savePlayerState();

    // Track play count in backend
    fetch('/Music/AddToTopTracks', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify({
            id: videoId,
            title: title,
            description: description || '',
            thumbnailUrl: thumbnailUrl || '',
            publishedAt: publishedAt || new Date().toISOString(),
            channelTitle: channelTitle || ''
        })
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        console.log('Successfully tracked video play:', data);
        // Update any UI that shows top tracks
        updateTopTracksUI();
    })
    .catch(error => {
        console.error('Error tracking video play:', error);
    });
}

// Function to update top tracks UI
function updateTopTracksUI() {
    const topTracksContainer = document.querySelector('.top-tracks .music-grid');
    if (!topTracksContainer) return;

    // Get played tracks from localStorage for guests
    const guestTracks = getPlayedTracks();
    if (guestTracks && guestTracks.length > 0) {
        // Find the video elements that match the guest tracks
        const videoCards = document.querySelectorAll('[data-video-id]');
        videoCards.forEach(card => {
            if (guestTracks.includes(card.dataset.videoId)) {
                // Move matching cards to the top tracks section
                const clone = card.cloneNode(true);
                topTracksContainer.appendChild(clone);
            }
        });
    }
}

function togglePlay() {
    if (!player || !isPlayerReady) return;
    
    const state = player.getPlayerState();
    if (state === YT.PlayerState.PLAYING) {
        player.pauseVideo();
    } else {
        player.playVideo();
    }
}

function nextSong() {
    if (playlist.length <= 1) return;
    currentIndex = (currentIndex + 1) % playlist.length;
    loadVideo(currentIndex, true);
}

function prevSong() {
    if (playlist.length <= 1) return;
    currentIndex = (currentIndex - 1 + playlist.length) % playlist.length;
    loadVideo(currentIndex, true);
}

function loadVideo(index, autoplay = false) {
    if (!player || !isPlayerReady || !playlist[index]) return;

    const id = playlist[index];
    const title = playlistTitles[index];
    const thumbnail = playlistThumbs[index];

    // Show loading state
    const loadingSpinner = document.getElementById('loading-spinner');
    if (loadingSpinner) {
        loadingSpinner.style.display = 'block';
    }

    // Update UI
    document.getElementById('player-thumbnail').src = thumbnail;
    document.getElementById('song-title').innerText = title;
    document.getElementById('player-bar').style.display = 'flex';

    // Load and play/pause video
    player.loadVideoById(id);
    if (!autoplay) {
        player.pauseVideo();
    }

    savePlayerState();

    // Hide loading spinner after a delay
    if (loadingSpinner) {
        setTimeout(() => {
            loadingSpinner.style.display = 'none';
        }, 1000);
    }
}

function startUpdatingProgress() {
    clearInterval(updateInterval);
    updateInterval = setInterval(() => {
        if (!player || !player.getDuration || !progressSlider || isSeeking) return;

        const duration = player.getDuration();
        const currentTime = player.getCurrentTime();
        
        progressSlider.max = duration;
        progressSlider.value = currentTime;

        const currentTimeEl = document.getElementById('current-time');
        const totalDurationEl = document.getElementById('total-duration');
        
        if (currentTimeEl && totalDurationEl) {
            currentTimeEl.textContent = formatTime(currentTime);
            totalDurationEl.textContent = formatTime(duration);
        }
    }, 500);
}

function formatTime(seconds) {
    const min = Math.floor(seconds / 60);
    const sec = Math.floor(seconds % 60).toString().padStart(2, '0');
    return `${min}:${sec}`;
}

// Initialize player controls on page load
document.addEventListener('DOMContentLoaded', () => {
    playPauseIcon = document.getElementById('play-pause-icon');
    progressSlider = document.getElementById('progress');

    if (progressSlider) {
        progressSlider.addEventListener('input', () => {
            isSeeking = true;
        });

        progressSlider.addEventListener('change', () => {
            if (player && isPlayerReady) {
                player.seekTo(progressSlider.value, true);
                isSeeking = false;
            }
        });
    }

    // Add click handlers for cards
    document.querySelectorAll('.card').forEach(card => {
        card.addEventListener('click', function() {
            const videoData = {
                id: this.dataset.videoId,
                title: this.dataset.title,
                description: this.dataset.description,
                thumbnailUrl: this.dataset.thumbnail,
                publishedAt: this.dataset.publishedAt,
                channelTitle: this.dataset.channelTitle
            };
            playVideo(videoData.id, videoData.title, videoData.description, 
                     this.querySelector('img').src, videoData.publishedAt, 
                     videoData.channelTitle);
        });
    });

    // Load saved state
    loadPlayerState();

    // Update top tracks UI on initial load
    updateTopTracksUI();
});
