﻿using System.Linq;
using Lastfm.Api;
using Lastfm.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Serialization;

namespace Lastfm.ServerEntryPoint
{
    /// <summary>
    ///     Class ServerEntryPoint
    /// </summary>
    public class LastfmServerEntryPoint : IServerEntryPoint
    {
        private readonly ISessionManager _sessionManager;
        private readonly IUserDataManager _userDataManager;
        private LastfmApi _lastfmApi;

        public LastfmServerEntryPoint(ISessionManager sessionManager, IJsonSerializer jsonSerializer,
            IHttpClient httpClient, IUserDataManager userDataManager)
        {
            _sessionManager = sessionManager;
            _userDataManager = userDataManager;
            _lastfmApi = new LastfmApi(httpClient, jsonSerializer);
            Instance = this;
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static LastfmServerEntryPoint Instance { get; private set; }

        /// <summary>
        ///     Runs this instance.
        /// </summary>
        public void Run()
        {
            //Bind events
            _sessionManager.PlaybackStart += PlaybackStart;
            _sessionManager.PlaybackStopped += PlaybackStopped;
            _userDataManager.UserDataSaved += UserDataSaved;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //Unbind events
            _sessionManager.PlaybackStart -= PlaybackStart;
            _sessionManager.PlaybackStopped -= PlaybackStopped;
            _userDataManager.UserDataSaved -= UserDataSaved;

            //Clean up
            _lastfmApi = null;
        }

        /// <summary>
        ///     Let last fm know when a user favourites or unfavourites a track
        /// </summary>
        private async void UserDataSaved(object sender, UserDataSaveEventArgs e)
        {
            //We only care about audio
            if(!(e.Item is Audio))
            {
                return;
            }

            //We also only care about User rating changes
            if(!e.SaveReason.Equals(UserDataSaveReason.UpdateUserRating))
            {
                return;
            }

            var lastfmUser = UserHelpers.GetUser(e.UserId);
            if(lastfmUser == null)
            {
                Plugin.Logger.Debug("Could not find user");
                return;
            }

            if(string.IsNullOrWhiteSpace(lastfmUser.SessionKey))
            {
                Plugin.Logger.Info("No session key present, aborting");
                return;
            }

            var item = (Audio) e.Item;

            //Dont do if syncing
            if(Plugin.Syncing)
            {
                return;
            }

            await _lastfmApi.TrackLove(item, lastfmUser, e.UserData.IsFavorite);
        }

        /// <summary>
        ///     Let last.fm know when a track has finished.
        ///     Playback stopped is run when a track is finished.
        /// </summary>
        private async void PlaybackStopped(object sender, PlaybackStopEventArgs e)
        {
            //We only care about audio
            if(!(e.Item is Audio))
            {
                return;
            }

            var item = (Audio) e.Item;

            //Make sure the track has been fully played
            if(!e.PlayedToCompletion)
            {
                Plugin.Logger.Debug("'{0}' not played to completion, not scrobbling", item.Name);
                return;
            }

            //Played to completion will sometimes be true even if the track has only played 10% so check the playback ourselfs (it must use the app settings or something)
            //Make sure 80% of the track has been played back
            if(e.PlaybackPositionTicks == null)
            {
                Plugin.Logger.Debug("Playback ticks for {0} is null", item.Name);
                return;
            }

            var playPercent = (double) e.PlaybackPositionTicks / item.RunTimeTicks * 100;
            if(playPercent < 80)
            {
                Plugin.Logger.Debug("'{0}' only played {1}%, not scrobbling", item.Name, playPercent);
                return;
            }

            var user = e.Users.FirstOrDefault();
            if(user == null)
            {
                Plugin.Logger.Debug("No user");
                return;
            }

            var lastfmUser = UserHelpers.GetUser(user);
            if(lastfmUser == null)
            {
                Plugin.Logger.Debug("Could not find last.fm user");
                return;
            }

            //User doesn't want to scrobble
            if(!lastfmUser.Scrobble)
            {
                Plugin.Logger.Debug("{0} ({1}) does not want to scrobble", user.Name, lastfmUser.Username);
                return;
            }

            if(string.IsNullOrWhiteSpace(lastfmUser.SessionKey))
            {
                Plugin.Logger.Info("No session key present, aborting");
                return;
            }

            await _lastfmApi.Scrobble(item, lastfmUser);
        }

        /// <summary>
        ///     Let Last.fm know when a user has started listening to a track
        /// </summary>
        private async void PlaybackStart(object sender, PlaybackProgressEventArgs e)
        {
            //We only care about audio
            if(!(e.Item is Audio))
            {
                return;
            }

            var user = e.Users.FirstOrDefault();
            if(user == null)
            {
                Plugin.Logger.Debug("No user");
                return;
            }

            var lastfmUser = UserHelpers.GetUser(user);
            if(lastfmUser == null)
            {
                Plugin.Logger.Debug("Could not find last.fm user");
                return;
            }

            //User doesn't want to scrobble
            if(!lastfmUser.Scrobble)
            {
                Plugin.Logger.Debug("{0} ({1}) does not want to scrobble", user.Name, lastfmUser.Username);
                return;
            }

            if(string.IsNullOrWhiteSpace(lastfmUser.SessionKey))
            {
                Plugin.Logger.Info("No session key present, aborting");
                return;
            }

            var item = (Audio) e.Item;
            await _lastfmApi.TrackNowPlaying(item, lastfmUser);
        }
    }
}