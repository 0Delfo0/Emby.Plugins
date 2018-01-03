using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Lastfm.Api.Model.Objects.Track;
using Lastfm.Resources;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Entities;

namespace Lastfm.Utils
{
    public static class Helpers
    {
        private static string CreateMd5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            var md5 = MD5.Create();

            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();

            foreach(byte b in hashBytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static void AppendSignature(ref Dictionary<string, string> data)
        {
            if(data.ContainsKey("api_sig"))
            {
                data.Remove("api_sig");
            }
            data.Add("api_sig", CreateSignature(data));
        }

        private static int ToTimestamp(DateTime date)
        {
            return Convert.ToInt32((date - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds);
        }

        public static DateTime FromTimestamp(double timestamp)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            return date.AddSeconds(timestamp).ToLocalTime();
        }

        public static int CurrentTimestamp()
        {
            return ToTimestamp(DateTime.Now);
        }

        public static string DictionaryToQueryString(Dictionary<string, string> data)
        {
            return string.Join("&", data.Where(
                k => !string.IsNullOrWhiteSpace(k.Value)).Select(kvp => string.Format("{0}={1}", Uri.EscapeUriString(kvp.Key), Uri.EscapeUriString(kvp.Value))));
        }

        private static string CreateSignature(Dictionary<string, string> data)
        {
            var s = new StringBuilder();

            foreach(var item in data.OrderBy(x => x.Key))
                s.Append(string.Format("{0}{1}", item.Key, item.Value));

            //Append seceret
            s.Append(PluginConst.LasfmApi.LastfmApiSeceret);

            return CreateMd5Hash(s.ToString());
        }

        //The nuget doesn't seem to have GetProviderId for artists
        public static string GetMusicBrainzArtistId(MusicArtist artist)
        {
            if(artist.ProviderIds == null)
            {
                Plugin.Logger.Debug("No provider id: {0}", artist.Name);
                return null;
            }

            if(artist.ProviderIds.TryGetValue(MetadataProviders.MusicBrainzArtist.ToString(), out var mbArtistId))
            {
                return mbArtistId;
            }
            Plugin.Logger.Debug("No GetMusicBrainzArtistId MBID: {0}", artist.Name);

            return null;
        }

        public static string GetMusicBrainzTrackId(Audio audio)
        {
            if(audio.ProviderIds == null)
            {
                Plugin.Logger.Debug("No provider id: {0}", audio.Name);
                return null;
            }

            if(audio.ProviderIds.TryGetValue(MetadataProviders.MusicBrainzTrack.ToString(), out var mbArtistId))
            {
                return mbArtistId;
            }
            Plugin.Logger.Debug("No GetMusicBrainzTrackId MBID: {0}", audio.Name);

            return null;
        }

        public static LfmTrack FindMatchedLastfmSong(IEnumerable<LfmTrack> tracks, Audio song)
        {
            return tracks.FirstOrDefault(lastfmTrack => IsLike(song.Name, lastfmTrack.name));
        }

        public static bool IsLike(string s, string t)
        {
            //Placeholder until we have a better way
            var source = SanitiseString(s);
            var target = SanitiseString(t);

            return source.Equals(target, StringComparison.OrdinalIgnoreCase);
        }

        private static string SanitiseString(string s)
        {
            //This coiuld also be [a-z][0-9]
            const string pattern = "[\\~#%&*{}/:<>?,-.()|\"-]";

            //Remove invalid chars and then all spaces
            return Regex.Replace(new Regex(pattern).Replace(s, string.Empty), @"\s+", string.Empty);
        }
    }
}