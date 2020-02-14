using System.Text.Json.Serialization;
using LiteDB;

namespace SpotifyPlaylistManagerWorker.Models
{
    public class Playlist
    {
        public ObjectId Id { get; set; }

        [JsonPropertyName("id")]
        public string PlaylistId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("public")]
        public bool Public { get; set; }

        public string TracksHref { get; set; }

        public int TracksCount { get; set; }
    }
}
