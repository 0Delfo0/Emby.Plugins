namespace Lastfm.Api.Model.Objects.Scrobble
{
    public class LfmScrobble : LfmBaseObject
    {
        public LfmScrobbleAttributes attr { get; set; }
    }

    public class LfmScrobbleAttributes
    {
        public bool accepted { get; set; }
        public bool ignored { get; set; }
    }
}