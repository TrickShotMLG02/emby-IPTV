using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Plugins;
using System;
using System.Collections.Generic;

namespace MediaBrowser.Channels.IPTV.Configuration
{
    /// <summary>
    /// Class PluginConfiguration
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {

        /// <summary>
        /// List of feeds
        /// </summary>
        /// <value>urls of xml podcast feeds</value>
        public Bookmark[] Bookmarks { get; set; }

        /// <summary>
        /// List of M3U playlists
        /// </summary>
        public List<M3UPlaylist> M3UPlaylists { get; set; } = new List<M3UPlaylist>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration" /> class.
        /// </summary>
        public PluginConfiguration()
        {
            Bookmarks = new Bookmark[] {};
        }
    }

    public class Bookmark
    {
        public String Name { get; set; }
        public String Image { get; set; }
        public String Path { get; set; }
        public MediaProtocol Protocol { get; set; }
        public String UserId { get; set; }
    }

    public class M3UPlaylist
    {
        public string Name { get; set; }
        public String Image { get; set; }
        public string Path { get; set; }
        public MediaProtocol Protocol { get; set; }
        public string UserId { get; set; }
    }
}
