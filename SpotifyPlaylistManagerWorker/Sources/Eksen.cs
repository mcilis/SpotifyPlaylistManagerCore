using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SpotifyPlaylistManagerWorker.Sources
{
    public class Eksen
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Eksen> _logger;
        private const string SourceServer = "EksenServer";

        public Eksen(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<Eksen> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetCurrentSongAsync()
        {
            // {"NowPlayingArtist":"","NowPlayingAlbum":null,"NowPlayingTrack":"WHITE TRASH BEAUTIFUL     ","NextPlayingArtist":"","NextPlayingAlbum":null,"NextPlayingTrack":"LE VENT NOUS "}

            using var client = _httpClientFactory.CreateClient(SourceServer);
            
            var request = new HttpRequestMessage(HttpMethod.Get, _configuration.GetValue("Source:Eksen:RequestUri", ""));

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var document = JsonDocument.Parse(responseContent, new JsonDocumentOptions { AllowTrailingCommas = true });

                    var artistName = document.RootElement.GetProperty("NowPlayingArtist").GetString();
                    var trackName = document.RootElement.GetProperty("NowPlayingTrack").GetString();
                    var song = $"{trackName.Trim().Replace(" ", "+")}+{artistName.Trim().Replace(" ", "+")}";

                    return song;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Response:{ResponseContent}", responseContent);
                    return "";
                }
            }

            _logger.LogError("Response:{StatusCode} | {ResponseContent}", response.StatusCode, responseContent);
            return "";
        }
    }
}