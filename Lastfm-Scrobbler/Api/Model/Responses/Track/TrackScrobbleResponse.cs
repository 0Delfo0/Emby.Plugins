using Lastfm.Api.Model.Objects.Scrobble;

namespace Lastfm.Api.Model.Responses
{
    public class TrackScrobbleResponse : BaseResponse
    {
        public LfmScrobble LfmScrobble { get; set; }
    }
}