using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lastfm.Api;
using Lastfm.Api.Model.Objects.Track;
using Lastfm.Configuration.Model;
using Lastfm.Resources;
using Lastfm.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace Lastfm.ScheduledTasks
{
    internal class LastfmSyncTask : IScheduledTask
    {
        private readonly IUserManager _userManager;
        private readonly LastfmApi _lastfmApi;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;

        public LastfmSyncTask(ILogManager loggerManager, IHttpClient httpClient, IJsonSerializer jsonSerializer, IUserManager userManager, IUserDataManager userDataManager)
        {
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = loggerManager.GetLogger(PluginConst.ThisPlugin.Name);
            _lastfmApi = new LastfmApi(httpClient, jsonSerializer, _logger);
        }

        public string Key => PluginConst.LastfmSyncTask.Key;
        public string Name => PluginConst.LastfmSyncTask.Name;
        public string Category => PluginConst.LastfmSyncTask.Category;
        public string Description => PluginConst.LastfmSyncTask.Description;

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new List<TaskTriggerInfo>();
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            //Get all users
            var users = _userManager.Users.Where(u =>
            {
                var user = UserHelpers.GetUser(u);

                return !string.IsNullOrWhiteSpace(user?.SessionKey);
            }).ToList();

            if(users.Count == 0)
            {
                _logger.Info("No users found");
                return;
            }

            Plugin.Syncing = true;

            var usersProcessed = 0;
            var totalUsers = users.Count;

            foreach(var user in users)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var progressOffset = (double) usersProcessed++ / totalUsers;
                var maxProgressForStage = (double) usersProcessed / totalUsers;

                await SyncDataforUserByArtistBulk(user, progress, cancellationToken, maxProgressForStage, progressOffset);
            }

            Plugin.Syncing = false;
        }

        private async Task SyncDataforUserByArtistBulk(User user, IProgress<double> progress, CancellationToken cancellationToken, double maxProgress, double progressOffset)
        {
            var artists = user.RootFolder.GetRecursiveChildren().OfType<MusicArtist>().ToList();
            var lastFmUser = UserHelpers.GetUser(user);

            var totalSongs = 0;
            var matchedSongs = 0;

            //Get loved tracks

            var lfmLovedTracks = await UserGetLovedTracks(lastFmUser, progress, cancellationToken, maxProgress, progressOffset);
            var hasLovedTracks = lfmLovedTracks.Any();

            //Get entire library
            var lfmTracks = await GetUsersLibrary(lastFmUser, progress, cancellationToken, maxProgress, progressOffset);

            if(lfmTracks.Any())
            {
                _logger.Info("User {0} has no tracks in last.fm", user.Name);
                return;
            }

            //Group the library by artist
            var userLibrary = lfmTracks.GroupBy(t => t.artist.mbid).ToList();

            //Loop through each artist
            foreach(var artist in artists)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //Get all the tracks by the current artist
                var artistMBid = Helpers.GetMusicBrainzArtistId(artist, _logger);

                if(artistMBid == null)
                    continue;

                //Get the tracks from lastfm for the current artist
                var artistTracks = userLibrary.FirstOrDefault(t => t.Key.Equals(artistMBid));

                if(artistTracks == null || !artistTracks.Any())
                {
                    _logger.Info("{0} has no tracks in last.fm library for {1}", user.Name, artist.Name);
                    continue;
                }

                var artistTracksList = artistTracks.ToList();

                _logger.Info("Found {0} tracks in last.fm library for {1}", artistTracksList.Count, artist.Name);

                //Loop through each song
                foreach(var song in artist.GetRecursiveChildren().OfType<Audio>())
                {
                    totalSongs++;

                    var matchedSong = Helpers.FindMatchedLastfmSong(artistTracksList, song);

                    if(matchedSong == null)
                        continue;

                    //We have found a match
                    matchedSongs++;

                    _logger.Debug("Found match for {0} = {1}", song.Name, matchedSong.name);

                    var userData = _userDataManager.GetUserData(user.Id, song);

                    //Check if its a favourite track
                    if(hasLovedTracks && lastFmUser.Options.SyncFavourites)
                    {
                        //Use MBID if set otherwise match on song name
                        var favourited = lfmLovedTracks.Any(
                            t => string.IsNullOrWhiteSpace(t.mbid)
                                ? Helpers.IsLike(t.name, matchedSong.name)
                                : t.mbid.Equals(matchedSong.mbid)
                        );

                        userData.IsFavorite = favourited;

                        _logger.Debug("{0} Favourite: {1}", song.Name, favourited);
                    }

                    //Update the play count
                    if(matchedSong.userplaycount > 0)
                    {
                        userData.Played = true;
                        userData.PlayCount = Math.Max(userData.PlayCount, matchedSong.userplaycount);
                    }

                    _userDataManager.SaveUserData(user.Id, song, userData, UserDataSaveReason.UpdateUserRating, cancellationToken);
                }
            }

            //The percentage might not actually be correct but I'm pretty tired and don't want to think about it
            _logger.Info("Finished import Last.fm library for {0}. Local Songs: {1} | Last.fm Songs: {2} | Matched Songs: {3} | {4}% match rate",
                user.Name, totalSongs, lfmTracks.Count, matchedSongs, Math.Round((double) matchedSongs / Math.Min(lfmTracks.Count, totalSongs) * 100));
        }

        private async Task<List<LfmTrack>> GetUsersLibrary(LfmUser lfmUser, IProgress<double> progress, CancellationToken cancellationToken, double maxProgress, double progressOffset)
        {
            //TODO
            // library.getArtists --> user.getArtistTracks --> track.getInfo

            var tracks = new List<LfmTrack>();
            var page = 1;
            bool moreTracks;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await _lastfmApi.GetTracks(lfmUser, cancellationToken, page++).ConfigureAwait(false);

                if(response == null || !response.HasTracks())
                    break;

                tracks.AddRange(response.Tracks.Tracks);

                moreTracks = !response.Tracks.Metadata.IsLastPage();

                //Only report progress in download because it will be 90% of the time taken
                var currentProgress = (double) response.Tracks.Metadata.Page / response.Tracks.Metadata.TotalPages * (maxProgress - progressOffset) + progressOffset;

                _logger.Debug("Progress: " + currentProgress * 100);

                progress.Report(currentProgress * 100);
            } while(moreTracks);

            return tracks;
        }

        private async Task<List<LfmLovedTrack>> UserGetLovedTracks(LfmUser lfmUser, IProgress<double> progress, CancellationToken cancellationToken, double maxProgress, double progressOffset)
        {
            var tracks = new List<LfmLovedTrack>();
            var page = 1;
            bool hasMorePage;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await _lastfmApi.UserGetLovedTracks(lfmUser, page++).ConfigureAwait(false);

                if(!response.HasLovedTracks())
                    break;

                tracks.AddRange(response.lovedTracks.track);

                hasMorePage = !response.lovedTracks.attr.IsLastPage();

                //Only report progress in download because it will be 90% of the time taken
                var currentProgress = (double) response.lovedTracks.attr.page / response.lovedTracks.attr.totalPages * (maxProgress - progressOffset) + progressOffset;

                _logger.Debug("Progress: " + currentProgress * 100);

                progress.Report(currentProgress * 100);
            } while(hasMorePage);

            return tracks;
        }
    }
}