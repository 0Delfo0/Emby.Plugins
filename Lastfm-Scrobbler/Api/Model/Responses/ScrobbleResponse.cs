using Lastfm.Api.Model.Objects.Scrobble;

namespace Lastfm.Api.Model.Responses
{
    public class ScrobbleResponse : BaseResponse
    {
        public LfmScrobble LfmScrobble { get; set; }
    }
}