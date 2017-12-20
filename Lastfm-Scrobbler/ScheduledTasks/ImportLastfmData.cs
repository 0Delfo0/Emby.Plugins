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
    internal class ImportLastfmData : IScheduledTask
    {
        private readonly IUserManager _userManager;
        private readonly LastfmApi _lastfmApi;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger _logger;

        public ImportLastfmData(ILogManager loggerManager, IHttpClient httpClient, IJsonSerializer jsonSerializer, IUserManager userManager, IUserDataManager userDataManager)
        {
            _userManager = userManager;
            _userDataManager = userDataManager;
            _logger = loggerManager.GetLogger(PluginConst.ThisPlugin.Name);
            _lastfmApi = new LastfmApi(httpClient, jsonSerializer, _logger);
        }

        public string Key => PluginConst.ImportDataTask.Key;
        public string Name => PluginConst.ImportDataTask.Name;
        public string Category => PluginConst.ImportDataTask.Category;
        public string Description => PluginConst.ImportDataTask.Description;

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
            var lovedTracksReponse = await _lastfmApi.GetLovedTracks(lastFmUser).ConfigureAwait(false);
            var hasLovedTracks = lovedTracksReponse.HasLovedTracks();

            //Get entire library
            var usersTracks = await GetUsersLibrary(lastFmUser, progress, cancellationToken, maxProgress, progressOffset);

            if(usersTracks.Count == 0)
            {
                _logger.Info("User {0} has no tracks in last.fm", user.Name);
                return;
            }

            //Group the library by artist
            var userLibrary = usersTracks.GroupBy(t => t.artist.mbid).ToList();

            //Loop through each artist
            foreach(var artist in artists)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //Get all the tracks by the current artist
                var artistMBid = Helpers.GetMusicBrainzArtistId(artist ,_logger);

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
                        var favourited = lovedTracksReponse.lovedTracks.track.Any(
                            t => string.IsNullOrWhiteSpace(t.mbid)
                                ? StringHelper.IsLike(t.name, matchedSong.name)
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
                    else
                    {
                        userData.Played = false;
                        userData.PlayCount = 0;
                        userData.LastPlayedDate = null;
                    }

                    _userDataManager.SaveUserData(user.Id, song, userData, UserDataSaveReason.UpdateUserRating, cancellationToken);
                }
            }

            //The percentage might not actually be correct but I'm pretty tired and don't want to think about it
            _logger.Info("Finished import Last.fm library for {0}. Local Songs: {1} | Last.fm Songs: {2} | Matched Songs: {3} | {4}% match rate",
                user.Name, totalSongs, usersTracks.Count, matchedSongs, Math.Round((double) matchedSongs / Math.Min(usersTracks.Count, totalSongs) * 100));
        }

        private async Task<List<LfmTrack>> GetUsersLibrary(LfmUser lfmUser, IProgress<double> progress, CancellationToken cancellationToken, double maxProgress, double progressOffset)
        {
            var tracks = new List<LfmTrack>();
            var page = 1; //Page 0 = 1
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
    }
}