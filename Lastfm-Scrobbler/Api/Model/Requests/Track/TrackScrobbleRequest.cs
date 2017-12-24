using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests
{
    public class TrackScrobbleRequest : BaseAuthedRequest
    {
        public string artist { get; set; }
        public string track { get; set; }
        public int timestamp { get; set; }
        public string album { get; set; }
        public string context { get; set; }
        public string streamId { get; set; }
        public string chosenByUser { get; set; }
        public int trackNumber { get; set; }
        public string mbid { get; set; }
        public string albumArtist { get; set; }
        public int duration { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(artist), artist},
                {nameof(track), track},
                {nameof(timestamp), timestamp.ToString()},
                {nameof(album), album},
                {nameof(context), context},
                {nameof(streamId), streamId},
                {nameof(chosenByUser), chosenByUser},
                {nameof(trackNumber), trackNumber.ToString()},
                {nameof(mbid), mbid},
                {nameof(albumArtist), albumArtist},
                {nameof(duration), duration.ToString()}
            };
        }
    }
}