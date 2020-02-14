using System.Collections.Generic;
using System.Text.Json.Serialization;
using LiteDB;

namespace SpotifyPlaylistManagerWorker.Models
{
    public class Track
    {
        public ObjectId Id { get; set; }

        public string Song { get; set; }

        [BsonRef("Playlist")]
        public Playlist Playlist { get; set; }

        [JsonPropertyName("id")]
        public string TrackId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; }
    }
}
