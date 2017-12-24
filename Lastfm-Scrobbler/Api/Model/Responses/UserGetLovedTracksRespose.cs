using System.Collections.Generic;
using System.Linq;
using Lastfm.Api.Model.Objects;
using Lastfm.Api.Model.Objects.Track;

namespace Lastfm.Api.Model.Responses
{
    public class UserGetLovedTracksRespose : BaseResponsePagedResponse
    {
        public LovedTracks lovedTracks { get; set; }
    }

    public class LovedTracks : IHasList<LfmLovedTrack>
    {
        public List<LfmLovedTrack> track { get; set; }
        public IPagedResponse @attr { get; set; }

        public bool HasElement(IEnumerable<LfmLovedTrack> list)
        {
            return list.Any();
        }

        public IEnumerable<LfmLovedTrack> GetElementsList(IEnumerable<LfmLovedTrack> list)
        {
            return list;
        }
    }
}