using Lastfm.Api;
using MediaBrowser.Common.Net;
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
            return _lastfmApi.AuthGetMobileSession(request.Username, request.Password);
        }
    }
}