using System.Collections.Generic;
using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses.User
{
    public class UserGetArtistTracksRespose : BaseResponsePagedResponse
    {
        public ArtistTracks artisttracks { get; set; }
    }

    public class ArtistTracks
    {
        public List<LfmTrack> track { get; set; }
        public IPagedResponse attr { get; set; }
    }
}