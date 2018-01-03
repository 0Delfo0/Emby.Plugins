using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses.Track
{
    public class TrackGetInfoResponse : BaseResponse
    {
        public LfmTrack track { get; set; }
    }
}