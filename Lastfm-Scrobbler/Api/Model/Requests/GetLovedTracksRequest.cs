using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests
{
    public class GetLovedTracksRequest : BaseRequest
    {
        public string user { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(user), user}
            };
        }
    }
}