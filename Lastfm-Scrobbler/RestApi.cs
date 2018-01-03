using System.Threading;
using System.Threading.Tasks;
using Lastfm.Api;
using Lastfm.Api.Model.Objects.Track;
using Lastfm.Configuration.Model;
using Lastfm.Resources;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;

namespace Lastfm
{
    [Route("/Lastfm/Login", "POST")]
    public class Login
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RestApi : IService
    {
        private readonly LastfmApi _lastfmApi;

        public RestApi(IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            _lastfmApi = new LastfmApi(httpClient, jsonSerializer);
        }

        public object Post(Login request)
        {
            var track = new LfmTrack
            {
                name = "Orion"
            };

            var user = new LfmUser
            {
                Username = "Delfo78"
            };

            var res =  _lastfmApi.TrackGetInfo(user, track, CancellationToken.None);

           

            Task.WaitAll(res);
            
            Plugin.Logger.Info("res {0}", res.Result.track.mbid);
            
            return _lastfmApi.AuthGetMobileSession(request.Username, request.Password).ConfigureAwait(false);
        }
    }
}