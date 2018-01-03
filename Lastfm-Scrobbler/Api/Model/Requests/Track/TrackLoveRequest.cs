using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests.Track
{
    public class TrackLoveRequest : BaseAuthedRequest
    {
        public string track { get; set; }
        public string artist { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(artist), artist},
                {nameof(track), track}
            };
        }
    }
}