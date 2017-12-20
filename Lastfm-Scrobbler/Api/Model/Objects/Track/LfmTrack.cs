using Lastfm.Api.Model.Objects.Track.Base;

namespace Lastfm.Api.Model.Objects.Track
{
    public class LfmTrack : BaseLfmTrack
    {
        public int playcount { get; set; }
        public int userplaycount { get; set; }
        
        
    }
}