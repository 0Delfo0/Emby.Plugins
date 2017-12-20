using Lastfm.Api;
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
        private readonly ILogger _logger;

        public RestApi(IJsonSerializer jsonSerializer, IHttpClient httpClient, ILogManager logManager)
        {
            _logger = logManager.GetLogger(PluginConst.ThisPlugin.Name);
            _lastfmApi = new LastfmApi(httpClient, jsonSerializer, _logger);
        }

        public object Post(Login request)
        {
            return _lastfmApi.RequestSession(request.Username, request.Password);
        }
    }
}