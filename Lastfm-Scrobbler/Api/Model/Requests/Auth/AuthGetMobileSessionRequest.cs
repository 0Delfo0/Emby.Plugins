using System.Collections.Generic;

namespace Lastfm.Api.Model.Requests
{
    public class AuthGetMobileSessionRequest : BaseAuthedRequest
    {
        public string password { get; set; }
        public string username { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary())
            {
                {nameof(password), password},
                {nameof(username), username}
            };
        }
    }
}