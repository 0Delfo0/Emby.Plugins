using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses
{
    public class GetTrackResponse : BaseResponse
    {
        public LfmTrack track { get; set; }
    }
}