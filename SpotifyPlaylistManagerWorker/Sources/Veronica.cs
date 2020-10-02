using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyPlaylistManagerWorker.Sources
{
    public class Veronica
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Veronica> _logger;
        private const string SourceServer = "VeronicaServer";

        public Veronica(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<Veronica> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetCurrentSongAsync()
        {
            using var client = _httpClientFactory.CreateClient(SourceServer);

            var request = new HttpRequestMessage(HttpMethod.Get, _configuration.GetValue("Source:Veronica:RequestUri", ""));
            request.Headers.Add("x-api-key", _configuration.GetValue("Source:Veronica:ApiKey", ""));

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var document = JsonDocument.Parse(responseContent, new JsonDocumentOptions { AllowTrailingCommas = true });

                    var stationsProperty = document.RootElement.GetProperty("data").GetProperty("getStations")[0].GetProperty("items");

                    var stationProperty = stationsProperty.EnumerateArray().FirstOrDefault(x => x.GetProperty("slug").GetString() == "radio-veronica");

                    var currentSongProperty = stationProperty.GetProperty("playouts")[0].GetProperty("track");

                    var artistName = currentSongProperty.GetProperty("artistName").GetString();
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