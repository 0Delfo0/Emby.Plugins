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
            if (user == null)
            {
                return null;
            }

            return Plugin.Instance.PluginConfiguration.LfmUsers == null ? null : GetUser(user.Id);
        }

        public static LfmUser GetUser(Guid userGuid)
        {
            return Plugin.Instance.PluginConfiguration.LfmUsers.FirstOrDefault(u =>
            {
                if (string.IsNullOrWhiteSpace(u.MediaBrowserUserId))
                {
                    return false;
                }

                Guid mediaBrowserUserIdGuid;
                if (Guid.TryParse(u.MediaBrowserUserId, out mediaBrowserUserIdGuid) && mediaBrowserUserIdGuid.Equals(userGuid))
                {
                    return true;
                }

                return false;
            });
        }

        public static LfmUser GetUser(string userGuid)
        {
            return Guid.TryParse(userGuid, out var g) ? GetUser(g) : null;
        }
    }
}