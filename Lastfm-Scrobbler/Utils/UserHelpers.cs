using System;
using System.Linq;
using Lastfm.Configuration.Model;
using MediaBrowser.Controller.Entities;

namespace Lastfm.Utils
{
    public static class UserHelpers
    {
        public static LfmUser GetUser(User user)
        {
            if(user == null)
                return null;

            return Plugin.Instance.PluginConfiguration.LfmUsers == null ? null : GetUser(user.Id);
        }

        private static LfmUser GetUser(Guid userId)
        {
            return Plugin.Instance.PluginConfiguration.LfmUsers.FirstOrDefault(u => u.MediaBrowserUserId.Equals(userId));
        }

        public static LfmUser GetUser(string userGuid)
        {
            return Guid.TryParse(userGuid, out Guid g) ? GetUser(g) : null;
        }
    }
}