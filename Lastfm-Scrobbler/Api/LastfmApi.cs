using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lastfm.Api.Model.Requests;
using Lastfm.Api.Model.Responses;
using Lastfm.Resources;
using Lastfm.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace Lastfm.Api
{
    public class LastfmApi : BaseLastfmApiClient
    {
        public LastfmApi(IHttpClient httpClient, IJsonSerializer jsonSerializer, ILogger logger) : base(httpClient, jsonSerializer, logger)
        {
        }

        public async Task<MobileSessionResponse> RequestSession(string username, string password)
        {
            //Build request object
            var request = new MobileSessionRequest
            {
                Username = username,
                Password = password,

                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = PluginConst.Methods.AuthGetMobileSession,
                Secure = true
            };

            var response = await Post<MobileSessionRequest, MobileSessionResponse>(request);

            //Log the key for debugging
            if(response != null)
                Logger.Info("{0} successfully logged into Last.fm", username);

            return response;
        }

        public async Task Scrobble(Audio item, LastfmUser user)
        {
            var request = new ScrobbleRequest
            {
                Track = item.Name,
                Album = item.Album,
                Artist = item.Artists.First(),
                Timestamp = Helpers.CurrentTimestamp(),

                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = PluginConst.Methods.TrackScrobble,
                SessionKey = user.SessionKey
            };

            var response = await Post<ScrobbleRequest, ScrobbleResponse>(request);

            if(response != null && !response.IsError())
            {
                Logger.Info("{0} played '{1}' - {2} - {3}", user.Username, request.Track, request.Album, request.Artist);
                return;
            }

            Logger.Error("Failed to TrackScrobble track: {0}", item.Name);
        }

        public async Task NowPlaying(Audio item, LastfmUser user)
        {
            var request = new NowPlayingRequest
            {
                Track = item.Name,
                Album = item.Album,
                Artist = item.Artists.First(),

                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = PluginConst.Methods.TrackUpdateNowPlaying,
                SessionKey = user.SessionKey
            };

            //Add duration
            if(item.RunTimeTicks != null)
                request.Duration = Convert.ToInt32(TimeSpan.FromTicks((long) item.RunTimeTicks).TotalSeconds);

            var response = await Post<NowPlayingRequest, ScrobbleResponse>(request);

            if(response != null && !response.IsError())
            {
                Logger.Info("{0} is now playing '{1}' - {2} - {3}", user.Username, request.Track, request.Album, request.Artist);
                return;
            }

            Logger.Error("Failed to send now playing for track: {0}", item.Name);
        }

        /// <summary>
        /// Loves or unloves a track
        /// </summary>
        /// <param name="item">The track</param>
        /// <param name="user">The Lastfm User</param>
        /// <param name="love">If the track is loved or not</param>
        /// <returns></returns>
        public async Task<bool> LoveTrack(Audio item, LastfmUser user, bool love = true)
        {
            var request = new TrackLoveRequest
            {
                Artist = item.Artists.First(),
                Track = item.Name,

                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = love ? PluginConst.Methods.TrackLove : PluginConst.Methods.TrackUnlove,
                SessionKey = user.SessionKey,
            };

            //Send the request
            var response = await Post<TrackLoveRequest, BaseResponse>(request);

            if(response != null && !response.IsError())
            {
                Logger.Info("{0} {2}loved track '{1}'", user.Username, item.Name, love ? "" : "un");
                return true;
            }

            Logger.Error("{0} Failed to love = {3} track '{1}' - {2}", user.Username, item.Name, response.Message, love);
            return false;
        }

        /// <summary>
        /// Unlove a track. This is the same as LoveTrack with love as false
        /// </summary>
        /// <param name="item">The track</param>
        /// <param name="user">The Lastfm User</param>
        /// <returns></returns>
        public async Task<bool> UnloveTrack(Audio item, LastfmUser user)
        {
            return await LoveTrack(item, user, false);
        }

        public async Task<LovedTracksResponse> GetLovedTracks(LastfmUser user)
        {
            var request = new GetLovedTracksRequest
            {
                User = user.Username,
                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = PluginConst.Methods.UserGetLovedTracks
            };

            return await Get<GetLovedTracksRequest, LovedTracksResponse>(request);
        }

        public async Task<GetTrackResponse> GetTracks(LastfmUser user, MusicArtist artist, CancellationToken cancellationToken)
        {
            var request = new GetTrackRequest
            {
                User = user.Username,
                Artist = artist.Name,
                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = PluginConst.Methods.GetTracks,
                Limit = 1000
            };

            return await Get<GetTrackRequest, GetTrackResponse>(request, cancellationToken);
        }

        public async Task<GetTrackResponse> GetTracks(LastfmUser user, CancellationToken cancellationToken, int page = 0, int limit = 200)
        {
            var request = new GetTrackRequest
            {
                User = user.Username,
                ApiKey = PluginConst.LasfmApi.LastfmApiKey,
                Method = PluginConst.Methods.GetTracks,
                Limit = limit,
                Page = page
            };

            return await Get<GetTrackRequest, GetTrackResponse>(request, cancellationToken);
        }
    }
}