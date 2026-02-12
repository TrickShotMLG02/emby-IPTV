using MediaBrowser.Controller.Channels;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.MediaInfo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaBrowser.Channels.IPTV
{
    public class IPTVPlaylist
    {
        public string Name { get; set; }
        public List<ChannelItemInfo> Items { get; set; } = new List<ChannelItemInfo>();

        public static IPTVPlaylist FromM3U(string m3uContent, string playlistName)
        {
            var playlist = new IPTVPlaylist { Name = playlistName };
            string[] lines = m3uContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            ChannelItemInfo currentItem = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("#EXTINF"))
                {
                    var tvgIdMatch = Regex.Match(line, "tvg-id=\"([^\"]+)\"");
                    string tvgId = tvgIdMatch.Success ? tvgIdMatch.Groups[1].Value : null;

                    var nameSplit = line.Split(new[] { ',' }, 2);
                    string name = nameSplit.Length == 2 ? nameSplit[1].Trim() : "Unknown";

                    currentItem = new ChannelItemInfo
                    {
                        Name = name,
                        Id = Guid.NewGuid().ToString("N"),
                        Type = ChannelItemType.Media,
                        ContentType = ChannelMediaContentType.Clip,
                        MediaType = ChannelMediaType.Video,
                    };
                }
                else if (!line.StartsWith("#") && currentItem != null)
                {
                    currentItem.MediaSources = new List<MediaSourceInfo>
                    {
                        new ChannelMediaInfo
                        {
                            Path = line.Trim(),
                            Protocol = MediaProtocol.Http
                        }.ToMediaSource()
                    };

                    playlist.Items.Add(currentItem);
                    currentItem = null;
                }
            }

            return playlist;
        }
    }
}
