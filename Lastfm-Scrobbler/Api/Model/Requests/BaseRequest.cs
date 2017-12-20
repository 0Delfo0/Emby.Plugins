using System.Collections.Generic;
using Lastfm.Resources;

namespace Lastfm.Api.Model.Requests
{
    public class BaseRequest
    {
        private static string api_key => PluginConst.LasfmApi.LastfmApiKey;
        public string method { get; set; }

        /// <summary>
        /// If the request is a secure request (Over HTTPS)
        /// </summary>
        public bool Secure { get; set; }

        public virtual Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                {nameof(api_key), api_key},
                {nameof(method), method}
            };
        }
    }

    public class BaseAuthedRequest : BaseRequest
    {
        public string sk { get; set; }
        public string api_sig { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(sk), sk},
                {nameof(api_sig), api_sig}
            };
        }
    }

    public interface IPagedRequest
    {
        int limit { get; set; }
        int page { get; set; }
    }
}