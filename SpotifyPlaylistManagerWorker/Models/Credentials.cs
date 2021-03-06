﻿using LiteDB;
using System;
using System.Text.Json.Serialization;

namespace SpotifyPlaylistManagerWorker.Models
{
    public class Credentials
    {
        public ObjectId Id { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
