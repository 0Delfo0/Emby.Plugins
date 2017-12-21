using System.Collections.Generic;
using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses
{
    public class UserGetLovedTracksRespose : BaseResponse
    {
        public LovedTracks lovedTracks { get; set; }

        public bool HasLovedTracks()
        {
            return lovedTracks?.track != null && lovedTracks.track?.Count > 0;
        }
    }

    public class LovedTracks
    {
        public List<LfmLovedTrack> track { get; set; }
        public UserGetLovedTracksAttr @attr { get; set; }
    }

    public class UserGetLovedTracksAttr
    {
        public string user { get; set; }
        public int page { get; set; }
        public int perPage { get; set; }
        public int totalPages { get; set; }
        public int total { get; set; }
        
        public bool IsLastPage()
        {
            return page.Equals(totalPages);
        }
    }
}