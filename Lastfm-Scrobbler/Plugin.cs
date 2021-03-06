﻿using System;
using System.Collections.Generic;
using System.Threading;
using Lastfm.Configuration;
using Lastfm.Resources;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Lastfm
{
    /// <summary>
    ///     Class Plugin
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public static ILogger Logger;
        internal static readonly SemaphoreSlim LastfmResourcePool = new SemaphoreSlim(4, 4);

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogManager logManager) : base(applicationPaths, xmlSerializer)
        {
            Logger = logManager.GetLogger(Name);
            Instance = this;
        }

        public PluginConfiguration PluginConfiguration => Configuration;

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Plugin Instance { get; private set; }

        /// <summary>
        ///     Flag set when an Import Syncing task is running
        /// </summary>
        public static bool Syncing { get; internal set; }

        /// <summary>
        ///     Gets the name of the plugin
        /// </summary>
        /// <value>The name.</value>
        public sealed override string Name => PluginConst.ThisPlugin.Name;

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => PluginConst.ThisPlugin.Description;

        public override Guid Id => PluginConst.ThisPlugin.Id;

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
    }
}