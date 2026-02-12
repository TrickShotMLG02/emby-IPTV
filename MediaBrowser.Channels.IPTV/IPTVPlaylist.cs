using MediaBrowser.Controller.Channels;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.MediaInfo;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var tempItems = new List<(ChannelItemInfo Item, string TvgId, int Resolution, string BaseName)>();
            ChannelItemInfo currentItem = null;
            string currentTvgId = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("#EXTINF"))
                {
                    var tvgIdMatch = Regex.Match(line, "tvg-id=\"([^\"]+)\"");
                    currentTvgId = tvgIdMatch.Success ? tvgIdMatch.Groups[1].Value : null;

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

                    // Store temporary info for deduplication
                    tempItems.Add((currentItem, currentTvgId, GetResolutionFromTitle(name), GetBaseChannelName(name)));
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

                    currentItem = null;
                    currentTvgId = null;
                }
            }

            // Deduplicate: prefer HD > SD, then highest resolution
            var deduped = tempItems
                .GroupBy(x =>
                {
                    // Extract base tvg-id without @HD/@SD
                    if (!string.IsNullOrEmpty(x.TvgId))
                        return Regex.Replace(x.TvgId, "@(HD|SD)$", "");
                    return x.BaseName; // fallback to name if no tvg-id
                })
                .Select(g => g
                    .OrderByDescending(x => GetQualityTier(x.TvgId))
                    .ThenByDescending(x => x.Resolution)
                    .First().Item)
                .ToList();

            playlist.Items.AddRange(deduped);

            return playlist;
        }

        // Helper: Extract quality number from the title, e.g., "3sat (1080p)" -> 1080
        private static int GetResolutionFromTitle(string title)
        {
            var match = Regex.Match(title, @"(\d+)p");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int res))
                return res;
            return 0; // unknown resolution
        }

        // Helper: Extract priority from tvg-id suffix: @HD = 2, @SD = 1, none = 0
        private static int GetQualityTier(string tvgId)
        {
            if (!string.IsNullOrEmpty(tvgId))
            {
                if (tvgId.EndsWith("@HD")) return 2;
                if (tvgId.EndsWith("@SD")) return 1;
            }
            return 0;
        }

        // Helper: Extract base channel name without resolution/quality info
        private static string GetBaseChannelName(string name)
        {
            return Regex.Replace(name, @"\s*\(\d+p\)", "").Trim();
        }
    }
}
