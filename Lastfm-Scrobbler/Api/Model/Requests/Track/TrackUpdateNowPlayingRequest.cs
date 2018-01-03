using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests.Track
{
    public class TrackUpdateNowPlayingRequest : BaseAuthedRequest
    {
        public string track { get; set; }
        public string album { get; set; }
        public string artist { get; set; }
        public int duration { get; set; }
        public int trackNumber { get; set; }
        public string context { get; set; }
        public string mbid { get; set; }
        public string albumArtist { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(track), track},
                {nameof(album), album},
                {nameof(artist), artist},
                {nameof(duration), duration.ToString()},
                {nameof(trackNumber), trackNumber.ToString()},
                {nameof(context), context},
                {nameof(mbid), mbid},
                {nameof(albumArtist), albumArtist}
            };
        }
    }
}