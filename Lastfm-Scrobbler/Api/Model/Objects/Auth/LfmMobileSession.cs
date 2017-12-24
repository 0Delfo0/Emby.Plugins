namespace Lastfm.Api.Model.Objects.Auth
{
    public class LfmMobileSession : LfmBaseObject
    {
        public string name { get; set; }
        public string key { get; set; }
        public int subscriber { get; set; }
    }
}