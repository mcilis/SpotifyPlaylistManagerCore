using System.Text.Json.Serialization;

namespace SpotifyPlaylistManagerWorker.Models
{
    public class Artist
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}
