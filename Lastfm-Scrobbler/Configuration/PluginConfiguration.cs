using Lastfm.Configuration.Model;
using MediaBrowser.Model.Plugins;

namespace Lastfm.Configuration
{
    /// <summary>
    ///     Class PluginConfiguration
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PluginConfiguration" /> class.
        /// </summary>
        public PluginConfiguration()
        {
            LfmUsers = new LfmUser[] { };
        }

        public LfmUser[] LfmUsers { get; }
    }
}