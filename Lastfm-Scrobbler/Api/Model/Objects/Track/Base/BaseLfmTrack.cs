using Lastfm.Api.Model.Objects.Artist;

namespace Lastfm.Api.Model.Objects.Track.Base
{
    public class BaseLfmTrack : LfmBaseObject
    {
        public LfmArtist artist { get; set; }
        public string name { get; set; }
        public string mbid { get; set; }
    }
}