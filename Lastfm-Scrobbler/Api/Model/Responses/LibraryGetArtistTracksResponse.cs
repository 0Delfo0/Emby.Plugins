using System.Collections.Generic;
using Lastfm.Api.Model.Objects.Artist;
using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses
{
    public class LibraryGetArtistTracksResponse : BaseResponsePagedResponse
    {
        public LfmArtists artists { get; set; }

        public bool HasLovedTracks()
        {
            return artists?.artist != null && artists.artist?.Count > 0;
        }
    }

    public class LfmArtists
    {
        public List<LfmArtist> artist { get; set; }
        public IPagedResponse @attr { get; set; }
    }

   
}