using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyPlaylistManagerWorker.Sources
{
    public class RedFm
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedFm> _logger;
        private const string SourceServer = "RedFmServer";

        public RedFm(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RedFm> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetCurrentSongAsync()
        {
            using var client = _httpClientFactory.CreateClient(SourceServer);

            var request = new HttpRequestMessage(HttpMethod.Get, _configuration.GetValue("Source:RedFm:RequestUri", ""));

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var document = JsonDocument.Parse(responseContent, new JsonDocumentOptions { AllowTrailingCommas = true });

                    var items = document.RootElement.GetProperty("feed").GetProperty("items").EnumerateArray();

                    foreach (var item in items)
                    {
                        var type = item.GetProperty("type").GetString();

                        if (type != "song")
                        {
                            continue;
                        }

                        var artistName = item.GetProperty("title").GetString();
                        var trackName = item.GetProperty("desc").GetString();
                        var song = $"{trackName.Trim().Replace(" ", "+")}+{artistName.Trim().Replace(" ", "+")}";

                        return song;
                    }
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