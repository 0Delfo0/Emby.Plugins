using Lastfm.Api.Model.Objects.Auth;

namespace Lastfm.Api.Model.Responses
{
    public class MobileSessionResponse : BaseResponse
    {
        public LfmMobileSession session { get; set; }
    }
}