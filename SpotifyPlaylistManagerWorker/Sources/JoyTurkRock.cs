using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyPlaylistManagerWorker.Sources
{
    public class JoyTurkRock
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JoyTurkRock> _logger;
        private const string SourceServer = "JoyTurkRockServer";

        public JoyTurkRock(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<JoyTurkRock> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetCurrentSongAsync()
        {
            using var client = _httpClientFactory.CreateClient(SourceServer);

            var request = new HttpRequestMessage(HttpMethod.Get, _configuration.GetValue("Source:JoyTurkRock:RequestUri", ""));

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var document = JsonDocument.Parse(responseContent, new JsonDocumentOptions { AllowTrailingCommas = true });

                    var currentSongProperty = document.RootElement.GetProperty("data").GetProperty("current_song");

                    var artistName = currentSongProperty.GetProperty("artist").GetString();
                    var trackName = currentSongProperty.GetProperty("title").GetString();
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