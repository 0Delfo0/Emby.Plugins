using System.Collections.Generic;
using System.Linq;
using Lastfm.Api.Model.Objects.Artist;

namespace Lastfm.Api.Model.Responses
{
    public class LibraryGetArtistTracksResponse : BaseResponsePagedResponse
    {
        public LfmArtists artists { get; set; }
    }

    public class LfmArtists
    {
        public List<LfmArtist> artist { get; set; }
        public IPagedResponse @attr { get; set; }
    }
}