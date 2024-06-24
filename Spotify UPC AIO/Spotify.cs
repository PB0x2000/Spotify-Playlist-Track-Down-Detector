using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Leaf.xNet;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using Console = Colorful.Console;
using static Spotify_UPC_AIO.Program;

namespace Spotify_UPC_AIO
{
    internal class Spotify
    {
        public static bool running = true;
        public static string auth = "";
        public static string basic = "";
        public static void reauth(string basic)
        {
            while (running)
            {
                Thread.Sleep(3500000);
                auth = Spotify.GetBearerToken(basic);
            }
        }

        public static string GetBearerToken(string basic)
        {
            HttpRequest request = new HttpRequest();
            request.AddHeader("Authorization", "Basic " + basic);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            string url = "https://accounts.spotify.com/api/token";
            string postData = "grant_type=client_credentials";
            HttpResponse response = request.Post(url, postData, "application/x-www-form-urlencoded");
            Auth_RESPONSE myDeserializedClass = JsonConvert.DeserializeObject<Auth_RESPONSE>(response.ToString());
            request.Dispose();
            return myDeserializedClass.access_token;
        }
        public static Track GetTrack(string id)
        {
            HttpRequest request = new HttpRequest();
            request.AddHeader("Authorization", "Bearer " + auth);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            string url = "https://api.spotify.com/v1/tracks/" + id;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Track myDeserializedClass = JsonConvert.DeserializeObject<Track>(request.Get(url).ToString(), settings);
            return myDeserializedClass;
        }
        public static Album GetAlbum(string id)
        {
            HttpRequest request = new HttpRequest();
            request.AddHeader("Authorization", "Bearer " + auth);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            string url = "https://api.spotify.com/v1/albums/" + id;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Album myDeserializedClass = JsonConvert.DeserializeObject<Album>(request.Get(url).ToString(), settings);
            return myDeserializedClass;
        }
        public static Playlist GetPlaylist(string id)
        {
            HttpRequest request = new HttpRequest();
            request.AddHeader("Authorization", "Bearer " + auth);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            string url = "https://api.spotify.com/v1/playlists/" + id;

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Playlist myDeserializedClass = JsonConvert.DeserializeObject<Playlist>(request.Get(url).ToString(), settings);
            return myDeserializedClass;
        }
        public static void GetAndAddRelatedArtists(string id)
        {
            HttpRequest request = new HttpRequest();
            request.AddHeader("Authorization", "Bearer " + auth);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            string url = "https://api.spotify.com/v1/artists/" + id + "/related-artists";

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Related myDeserializedClass = JsonConvert.DeserializeObject<Related>(request.Get(url).ToString(), settings);

            foreach(RelatedArtist artist in myDeserializedClass.artists)
            {
                if (!DB_ARTISTS.Exists(x => x.ID == artist.id))
                {
                    DB_ARTIST db_artist = new DB_ARTIST();
                    db_artist.NAME = artist.name;
                    db_artist.ID = artist.id;
                    db_artist.LAST_CHECKED = "null";
                    DB_ARTISTS.Add(db_artist);
                    added++;
                }
                else
                {
                    DB_ARTIST db_artist = DB_ARTISTS.First(x => x.ID == artist.id);
                    db_artist.LAST_CHECKED = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                }
            }
        }

        private class Auth_RESPONSE
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        #region GET_RELATED_ARTISTS_RESPONSE
        public class RelatedArtist
        {
            public RelatedExternalUrls external_urls { get; set; }
            public RelatedFollowers followers { get; set; }
            public List<string> genres { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<RelatedImage> images { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }
        public class RelatedExternalUrls
        {
            public string spotify { get; set; }
        }
        public class RelatedFollowers
        {
            public object href { get; set; }
            public int total { get; set; }
        }
        public class RelatedImage
        {
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }
        public class Related
        {
            public List<RelatedArtist> artists { get; set; }
        }
        #endregion

        #region GET_TRACK_RESPONSE
        public class TrackAlbum
        {
            public string album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string> available_markets { get; set; }
            public TrackExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<TrackImage> images { get; set; }
            public string name { get; set; }
            public string release_date { get; set; }
            public string release_date_precision { get; set; }
            public TrackRestrictions restrictions { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public List<TrackCopyright> copyrights { get; set; }
            public TrackExternalIds external_ids { get; set; }
            public List<string> genres { get; set; }
            public string label { get; set; }
            public int popularity { get; set; }
            public string album_group { get; set; }
            public List<TrackArtist> artists { get; set; }
        }
        public class TrackArtist
        {
            public TrackExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public TrackFollowers followers { get; set; }
            public List<string> genres { get; set; }
            public List<TrackImage> images { get; set; }
            public int popularity { get; set; }
        }
        public class TrackCopyright
        {
            public string text { get; set; }
            public string type { get; set; }
        }
        public class TrackExternalIds
        {
            public string isrc { get; set; }
            public string ean { get; set; }
            public string upc { get; set; }
        }
        public class TrackExternalUrls
        {
            public string spotify { get; set; }
        }
        public class TrackFollowers
        {
            public string href { get; set; }
            public int total { get; set; }
        }
        public class TrackImage
        {
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }
        public class TrackLinkedFrom
        {
        }
        public class TrackRestrictions
        {
            public string reason { get; set; }
        }
        public class Track
        {
            public TrackAlbum album { get; set; }
            public List<TrackArtist> artists { get; set; }
            public List<string> available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            public bool @explicit { get; set; }
            public TrackExternalIds external_ids { get; set; }
            public TrackExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public bool is_playable { get; set; }
            public TrackLinkedFrom linked_from { get; set; }
            public TrackRestrictions restrictions { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public bool is_local { get; set; }
        }
        #endregion

        #region GET_ALBUM_RESPONSE
        public class AlbumArtist
        {
            public AlbumExternalUrls external_urls { get; set; }
            public AlbumFollowers followers { get; set; }
            public List<string> genres { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<AlbumImage> images { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }
        public class AlbumCopyright
        {
            public string text { get; set; }
            public string type { get; set; }
        }
        public class AlbumExternalIds
        {
            public string isrc { get; set; }
            public string ean { get; set; }
            public string upc { get; set; }
        }
        public class AlbumExternalUrls
        {
            public string spotify { get; set; }
        }
        public class AlbumFollowers
        {
            public string href { get; set; }
            public int total { get; set; }
        }
        public class AlbumImage
        {
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }
        public class AlbumItem
        {
            public List<AlbumArtist> artists { get; set; }
            public List<string> available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            public bool @explicit { get; set; }
            public AlbumExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public bool is_playable { get; set; }
            public AlbumLinkedFrom linked_from { get; set; }
            public AlbumRestrictions restrictions { get; set; }
            public string name { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public bool is_local { get; set; }
        }
        public class AlbumLinkedFrom
        {
            public AlbumExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }
        public class AlbumRestrictions
        {
            public string reason { get; set; }
        }
        public class Album
        {
            public string album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string> available_markets { get; set; }
            public AlbumExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<AlbumImage> images { get; set; }
            public string name { get; set; }
            public string release_date { get; set; }
            public string release_date_precision { get; set; }
            public AlbumRestrictions restrictions { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public List<AlbumCopyright> copyrights { get; set; }
            public AlbumExternalIds external_ids { get; set; }
            public List<string> genres { get; set; }
            public string label { get; set; }
            public int popularity { get; set; }
            public List<AlbumArtist> artists { get; set; }
            public AlbumTracks tracks { get; set; }
        }
        public class AlbumTracks
        {
            public string href { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public string previous { get; set; }
            public int total { get; set; }
            public List<AlbumItem> items { get; set; }
        }
        #endregion

        #region GET_PLAYLIST_RESPONSE
        public class PlaylistAddedBy
        {
            public PlaylistExternalUrls external_urls { get; set; }
            public PlaylistFollowers followers { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }
        public class PlaylistAlbum
        {
            public string album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string> available_markets { get; set; }
            public PlaylistExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<PlaylistImage> images { get; set; }
            public string name { get; set; }
            public string release_date { get; set; }
            public string release_date_precision { get; set; }
            public PlaylistRestrictions restrictions { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public List<PlaylistCopyright> copyrights { get; set; }
            public PlaylistExternalIds external_ids { get; set; }
            public List<string> genres { get; set; }
            public string label { get; set; }
            public int popularity { get; set; }
            public string album_group { get; set; }
            public List<PlaylistArtist> artists { get; set; }
        }
        public class PlaylistArtist
        {
            public PlaylistExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public PlaylistFollowers followers { get; set; }
            public List<string> genres { get; set; }
            public List<PlaylistImage> images { get; set; }
            public int popularity { get; set; }
        }
        public class PlaylistCopyright
        {
            public string text { get; set; }
            public string type { get; set; }
        }
        public class PlaylistExternalIds
        {
            public string isrc { get; set; }
            public string ean { get; set; }
            public string upc { get; set; }
        }
        public class PlaylistExternalUrls
        {
            public string spotify { get; set; }
        }
        public class PlaylistFollowers
        {
            public string href { get; set; }
            public int total { get; set; }
        }
        public class PlaylistImage
        {
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }
        public class PlaylistItem
        {
            public string added_at { get; set; }
            public PlaylistAddedBy added_by { get; set; }
            public bool is_local { get; set; }
            public PlaylistTrack track { get; set; }
        }
        public class PlaylistLinkedFrom
        {
        }
        public class PlaylistOwner
        {
            public PlaylistExternalUrls external_urls { get; set; }
            public PlaylistFollowers followers { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public string display_name { get; set; }
        }
        public class PlaylistRestrictions
        {
            public string reason { get; set; }
        }
        public class Playlist
        {
            public bool collaborative { get; set; }
            public string description { get; set; }
            public PlaylistExternalUrls external_urls { get; set; }
            public PlaylistFollowers followers { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<PlaylistImage> images { get; set; }
            public string name { get; set; }
            public PlaylistOwner owner { get; set; }
            public bool @public { get; set; }
            public string snapshot_id { get; set; }
            public PlaylistTracks tracks { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }
        public class PlaylistTrack
        {
            public PlaylistAlbum album { get; set; }
            public List<PlaylistArtist> artists { get; set; }
            public List<string> available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            public bool @explicit { get; set; }
            public PlaylistExternalIds external_ids { get; set; }
            public PlaylistExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public bool is_playable { get; set; }
            public PlaylistLinkedFrom linked_from { get; set; }
            public PlaylistRestrictions restrictions { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public bool is_local { get; set; }
        }
        public class PlaylistTracks
        {
            public string href { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public string previous { get; set; }
            public int total { get; set; }
            public List<PlaylistItem> items { get; set; }
        }
        #endregion
    }
}
