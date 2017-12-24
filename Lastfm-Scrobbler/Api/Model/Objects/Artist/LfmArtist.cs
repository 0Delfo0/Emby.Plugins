namespace Lastfm.Api.Model.Objects.Artist
{
    public class LfmArtist : LfmBaseObject
    {
        public string name { get; set; }
        public string mbid { get; set; }
        public int playcount { get; set; }
        public int tagcount { get; set; }
        public int streamable { get; set; }
    }
}