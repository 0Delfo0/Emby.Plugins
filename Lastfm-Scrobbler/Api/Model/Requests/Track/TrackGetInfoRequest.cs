using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests.Track
{
    public class TrackGetInfoRequest : BaseRequest
    {
        public string mbid { get; set; }
        public string track { get; set; }
        public string artist { get; set; }
        public string username { get; set; }
        public int autocorrect { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(mbid), mbid},
                {nameof(track), track},
                {nameof(artist), artist},
                {nameof(username), username},
                {nameof(autocorrect), autocorrect.ToString()}
            };
        }
    }
}