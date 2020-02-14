using LiteDB;
using Microsoft.Extensions.Configuration;
using SpotifyPlaylistManagerWorker.Models;
using System.Linq;

namespace SpotifyPlaylistManagerWorker.Repositories
{
    public class TrackRepository
    {
        private readonly string _db;

        public TrackRepository(IConfiguration configuration)
        {
            _db = configuration.GetValue<string>("LiteDb:DatabaseLocation");
        }

        public bool Exists(Playlist playlist, string song)
        {
            if (playlist == null || string.IsNullOrWhiteSpace(song))
                return false;

            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Track>("Track");

            var exists = collection.Exists(x => x.Playlist == playlist && x.Song == song);

            return exists;
        }

        public Track Retrieve(Playlist playlist, string song)
        {
            if (playlist == null || string.IsNullOrWhiteSpace(song))
                return null;

            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Track>("Track");

            var track = collection.Find(x => x.Playlist == playlist && x.Song == song).FirstOrDefault();

            return track;
        }

        public void Register(Track track)
        {
            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Track>("Track");

            collection.EnsureIndex(x => x.Song);

            collection.Upsert(track);
        }
    }
}
