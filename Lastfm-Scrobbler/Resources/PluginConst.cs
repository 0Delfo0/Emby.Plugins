using System;

namespace Lastfm.Resources
{
    public static class PluginConst
    {
        private static readonly Guid Guid = new Guid("82b5fb21-e174-40a1-a335-d1a19b03d175");

        internal static class Methods
        {
            internal const string TrackScrobble = "track.LfmScrobble";
            internal const string TrackUpdateNowPlaying = "track.updateNowPlaying";
            internal const string AuthGetMobileSession = "auth.getMobileSession";
            internal const string TrackLove = "track.love";
            internal const string TrackUnlove = "track.unlove";
            internal const string UserGetLovedTracks = "user.getLovedTracks";
            internal const string TrackGetInfo = "track.getInfo";
        }

        internal static class LasfmApi
        {
            internal const string LastfmBaseUrl = "ws.audioscrobbler.com";
            internal const string ApiVersion = "2.0";
            internal const string LastfmApiKey = "cb3bdcd415fcb40cd572b137b2b255f5";
            internal const string LastfmApiSeceret = "3a08f9fad6ddc4c35b0dce0062cecb5e";
        }

        internal static class ThisPlugin
        {
            internal const string Name = "Lastfm";
            internal const string Description = "Lastfm";
            internal static Guid Id { get; } = Guid;
        }
        
        internal static class Scrobbler
        {
            internal const string Name = "Last.fm Scrobbler";
        }

        internal static class ImportDataTask
        {
            internal const string Name = "Import Last.fm Data";
            internal const string Key = "ImportDataTask_Key";
            internal const string Category = "Last.fm";
            internal const string Description = "Import play counts and favourite tracks for each user with Last.fm accounted configured";
        }
    }
}