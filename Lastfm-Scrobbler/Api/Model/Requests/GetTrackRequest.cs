using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests
{
    public class GetTrackRequest : BaseRequest
    {
        public string user { get; set; }

        public string artist { get; set; }

//        public int limit { get; set; }
        //      public int page { get; set; }
        public string username { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(user), user},
                {nameof(artist), artist},
                {nameof(username), username}
            };
        }
    }
}