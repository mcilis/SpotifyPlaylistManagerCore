using LiteDB;
using Microsoft.Extensions.Configuration;
using SpotifyPlaylistManagerWorker.Models;
using System.Linq;

namespace SpotifyPlaylistManagerWorker.Repositories
{
    public class PlaylistRepository
    {
        private readonly string _db;

        public PlaylistRepository(IConfiguration configuration)
        {
            _db = configuration.GetValue<string>("LiteDb:DatabaseLocation");
        }

        public Playlist Retrieve(string playlistName)
        {
            if (string.IsNullOrWhiteSpace(playlistName))
                return null;

            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Playlist>("Playlist");

            var playlist = collection.Find(x=> x.Name == playlistName).FirstOrDefault();

            return playlist;
        }

        public void Register(Playlist playlist)
        {
            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Playlist>("Playlist");

            collection.EnsureIndex(x => x.Name);

            collection.Upsert(playlist);
        }
    }
}
