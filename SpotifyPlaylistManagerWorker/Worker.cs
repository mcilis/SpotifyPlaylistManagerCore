using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpotifyPlaylistManagerWorker.Models;
using SpotifyPlaylistManagerWorker.Repositories;
using SpotifyPlaylistManagerWorker.Sources;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpotifyPlaylistManagerWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Spotify _spotify;
        private readonly Eksen _eksen;
        private readonly JoyFm _joyFm;
        private readonly JoyTurkRock _joyTurkRock;
        private readonly RedFm _redFm;
        private readonly Veronica _veronica;
        private readonly VeronicaRock _veronicaRock;
        private readonly PlaylistRepository _playlistRepository;
        private readonly TrackRepository _trackRepository;

        public Worker(ILogger<Worker> logger, Spotify spotify, Eksen eksen, JoyFm joyFm, JoyTurkRock joyTurkRock, RedFm redFm, Veronica veronica, VeronicaRock veronicaRock, PlaylistRepository playlistRepository, TrackRepository trackRepository)
        {
            _logger = logger;
            _spotify = spotify;
            _eksen = eksen;
            _joyFm = joyFm;
            _joyTurkRock = joyTurkRock;
            _redFm = redFm;
            _veronica = veronica;
            _veronicaRock = veronicaRock;
            _playlistRepository = playlistRepository;
            _trackRepository = trackRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await AddSongToPlaylist(await GetPlaylist($"Eksen {DateTime.UtcNow:MMMM yyyy}"), await _eksen.GetCurrentSongAsync());
                    await AddSongToPlaylist(await GetPlaylist($"Joy {DateTime.UtcNow:MMMM yyyy}"), await _joyFm.GetCurrentSongAsync());
                    await AddSongToPlaylist(await GetPlaylist($"Joy Turk Rock {DateTime.UtcNow:MMMM yyyy}"), await _joyTurkRock.GetCurrentSongAsync());
                    await AddSongToPlaylist(await GetPlaylist($"Red {DateTime.UtcNow:MMMM yyyy}"), await _redFm.GetCurrentSongAsync());
                    await AddSongToPlaylist(await GetPlaylist($"Veronica {DateTime.UtcNow:MMMM yyyy}"), await _veronica.GetCurrentSongAsync());
                    await AddSongToPlaylist(await GetPlaylist($"Veronica Rock {DateTime.UtcNow:MMMM yyyy}"), await _veronicaRock.GetCurrentSongAsync());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Worker Error!!!");
                    throw;
                }

                if (DateTime.UtcNow.Hour > 22)
                {
                    await Task.Delay(28800000, stoppingToken); // wait 8 hours for the new song
                }

                await Task.Delay(240000, stoppingToken); // wait 4 minutes for the new song
            }
        }

        private async Task<Playlist> GetPlaylist(string playlistName)
        {
            var playlist = _playlistRepository.Retrieve(playlistName);

            if (playlist != null) 
                return playlist;

            playlist = await _spotify.FindPlaylistAsync(playlistName) ?? await _spotify.CreatePlaylistAsync(playlistName);

            if (playlist != null)
            {
                _playlistRepository.Register(playlist);
            }

            return playlist;
        }

        private async Task AddSongToPlaylist(Playlist playlist, string song, bool checkExistence = true)
        {
            if (playlist == null || song == null)
                return;

            if (checkExistence && _trackRepository.Exists(playlist, song))
                return;

            var track = await _spotify.SearchForATrackAsync(song);

            if (track == null)
            {
                _logger.LogWarning("{SearchedSong} : [SpotifyFileNotFound!]", song);

                track = new Track {Song = song, Playlist = playlist};
                _trackRepository.Register(track); // To avoid searching for the track again and again
                return;
            }

            if (await _spotify.AddTrackToPlaylistAsync(playlist, track))
            {
                _logger.LogInformation("Spotify track is added. Playlist:{PlaylistName} Artist:{ArtistName} Track:{TrackName}", 
                    playlist.Name, track.Artists.FirstOrDefault()?.Name, track.Name);

                track.Song = song;
                track.Playlist = playlist;
                _trackRepository.Register(track);
            }
        }
    }
}
