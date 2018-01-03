using System;

namespace Lastfm.Configuration.Model
{
    public class LfmUser
    {
        public string Username { get; set; }

        //We wont store the password, but instead store the session key since its a lifetime key
        public string SessionKey { get; set; }

        public String MediaBrowserUserId { get; set; }

        public bool Scrobble { get; set; }
        public bool SyncFavourites { get; set; }

    }
}