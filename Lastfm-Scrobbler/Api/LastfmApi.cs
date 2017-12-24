using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lastfm.Api.Model.Objects.Artist;
using Lastfm.Api.Model.Objects.Track;
using Lastfm.Api.Model.Requests;
using Lastfm.Api.Model.Responses;
using Lastfm.Configuration.Model;
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

        public async Task<AuthGetMobileSessionResponse> RequestSession(string username, string password)
        {
            //Build request object
            var request = new AuthGetMobileSessionRequest
            {
                username = username,
                password = password,
                method = PluginConst.Methods.Auth.GetMobileSession,
                Secure = true
            };

            var response = await Post<AuthGetMobileSessionRequest, AuthGetMobileSessionResponse>(request);

            //Log the key for debugging
            if(response != null)
                Logger.Info("{0} successfully logged into Last.fm", username);

            return response;
        }

        public async Task Scrobble(Audio item, LfmUser user)
        {
            var request = new TrackScrobbleRequest
            {
                track = item.Name,
                album = item.Album,
                artist = item.Artists.First(),
                timestamp = Helpers.CurrentTimestamp(),
                mbid = Helpers.GetMusicBrainzTrackId(item, Logger),
                // TODO 
                //trackNumber = item.get
                method = PluginConst.Methods.Track.Scrobble,
                sk = user.SessionKey
            };

            var response = await Post<TrackScrobbleRequest, TrackScrobbleResponse>(request);

            if(response != null && !response.IsError())
            {
                Logger.Info("{0} played '{1}' - {2} - {3} - {4}", user.Username, request.track, request.album, request.artist, request.mbid);
                return;
            }
            Logger.Error("Failed to Scrobble track: {0} - messagge {1}", item.Name, response?.message);
        }

        public async Task TrackNowPlaying(Audio item, LfmUser user)
        {
            var request = new TrackUpdateNowPlayingRequest
            {
                track = item.Name,
                album = item.Album,
                artist = item.Artists.First(),
                mbid = Helpers.GetMusicBrainzTrackId(item, Logger),
                method = PluginConst.Methods.Track.UpdateNowPlaying,
                sk = user.SessionKey
            };

            //Add duration
            if(item.RunTimeTicks != null)
                request.duration = Convert.ToInt32(TimeSpan.FromTicks((long) item.RunTimeTicks).TotalSeconds);

            var response = await Post<TrackUpdateNowPlayingRequest, TrackScrobbleResponse>(request);

            if(response != null && !response.IsError())
            {
                Logger.Info("{0} is now playing '{1}' - {2} - {3} - {4}", user.Username, request.track, request.album, request.artist, request.mbid);
                return;
            }
            Logger.Error("Failed to send now playing for track: {0} - messagge {1}", item.Name, response?.message);
        }

        /// <summary>
        /// Loves or unloves a track
        /// </summary>
        /// <param name="item">The track</param>
        /// <param name="user">The Lastfm User</param>
        /// <param name="love">If the track is loved or not</param>
        /// <returns></returns>
        public async Task<bool> TrackLove(Audio item, LfmUser user, bool love = true)
        {
            var request = new TrackLoveRequest
            {
                artist = item.Artists.First(),
                track = item.Name,
                method = love ? PluginConst.Methods.Track.Love : PluginConst.Methods.Track.Unlove,
                sk = user.SessionKey
            };

            //Send the request
            var response = await Post<TrackLoveRequest, BaseResponse>(request);

            if(response != null && !response.IsError())
            {
                Logger.Info("{0} {2}loved track '{1}'", user.Username, item.Name, love ? "" : "un");
                return true;
            }

            Logger.Error("{0} Failed to love = {3} track '{1}' - {2}", user.Username, item.Name, response?.message, love);
            return false;
        }

        /// <summary>
        /// Unlove a track. This is the same as LoveTrack with love as false
        /// </summary>
        /// <param name="item">The track</param>
        /// <param name="user">The Lastfm User</param>
        /// <returns></returns>
        public async Task<bool> TrackUnlove(Audio item, LfmUser user)
        {
            return await TrackLove(item, user, false);
        }

        public async Task<UserGetArtistTracksRespose> UserGetArtistTracks(LfmUser user, LfmArtist artist, int page = 0)
        {
            var request = new UserGetArtistTracksRequest
            {
                user = user.Username,
                artist = artist.name,
                page = page,
                limit = 0,
                method = PluginConst.Methods.User.GetArtistTracks
            };

            return await Get<UserGetArtistTracksRequest, UserGetArtistTracksRespose>(request);
        }

        public async Task<UserGetLovedTracksRespose> UserGetLovedTracks(LfmUser user, int page = 0, int limit = 200)
        {
            var request = new UserGetLovedTracksRequest
            {
                user = user.Username,
                page = page,
                limit = limit,
                method = PluginConst.Methods.User.GetLovedTracks
            };

            return await Get<UserGetLovedTracksRequest, UserGetLovedTracksRespose>(request);
        }

        public async Task<LibraryGetArtistTracksResponse> LibraryGetArtistTracks(LfmUser user, int page = 0, int limit = 200)
        {
            var request = new LibraryGetArtistTracksRequest
            {
                user = user.Username,
                page = page,
                limit = limit,
                method = PluginConst.Methods.User.GetLovedTracks
            };

            return await Get<LibraryGetArtistTracksRequest, LibraryGetArtistTracksResponse>(request);
        }

        public async Task<TrackGetInfoResponse> TrackGetInfo(LfmUser user, LfmTrack track, CancellationToken cancellationToken)
        {
            var request = new TrackGetInfoRequest
            {
                mbid = track.mbid,
                username = user.Username,
                track = track.name,
                artist = track.artist?.name,
                method = PluginConst.Methods.Track.GetInfo
            };

            return await Get<TrackGetInfoRequest, TrackGetInfoResponse>(request, cancellationToken);
        }
    }
}