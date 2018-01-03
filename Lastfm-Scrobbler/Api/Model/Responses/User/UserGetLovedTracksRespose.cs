using System.Collections.Generic;
using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses.User
{
    public class UserGetLovedTracksRespose : BaseResponsePagedResponse
    {
        public LovedTracks lovedTracks { get; set; }
    }

    public class LovedTracks
    {
        public List<LfmLovedTrack> track { get; set; }
        public IPagedResponse attr { get; set; }
    }
}