namespace Lastfm.Api.Model.Objects.Album
{
    public class LfmAlbum : LfmBaseObject
    {
        public string artist { get; set; }
        public string title { get; set; }
        public string mbid { get; set; }
    }
}