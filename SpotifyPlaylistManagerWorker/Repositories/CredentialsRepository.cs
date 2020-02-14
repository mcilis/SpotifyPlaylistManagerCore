using LiteDB;
using Microsoft.Extensions.Configuration;
using SpotifyPlaylistManagerWorker.Models;
using System.Linq;

namespace SpotifyPlaylistManagerWorker.Repositories
{
    public class CredentialsRepository
    {
        private readonly string _db;

        public CredentialsRepository(IConfiguration configuration)
        {
            _db = configuration.GetValue<string>("LiteDb:DatabaseLocation");
        }

        public Credentials Retrieve()
        {
            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Credentials>("Credentials");

            var credentials = collection.FindAll().FirstOrDefault();

            return credentials;
        }
        
        public void Register(Credentials credentials)
        {
            using var db = new LiteDatabase(_db);

            var collection = db.GetCollection<Credentials>("Credentials");

            collection.Upsert(credentials);
        }
    }
}
