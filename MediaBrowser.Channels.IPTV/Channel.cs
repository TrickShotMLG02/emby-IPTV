using MediaBrowser.Channels.IPTV.Configuration;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Channels.IPTV
{
    class Channel : IChannel, IHasCacheKey, IHasChangeEvent
    {
        private readonly ILogger _logger;

        public event EventHandler ContentChanged;

        public void OnContentChanged()
        {
            if (ContentChanged != null)
            {
                ContentChanged(this, EventArgs.Empty);
            }
        }

        public Channel(ILogManager logManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
        }

        public string DataVersion
        {
            get
            {
                // Increment as needed to invalidate all caches
                return "1";
            }
        }

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.Debug("cat ID : " + query.FolderId);

            return await GetChannelItemsInternal(query, cancellationToken).ConfigureAwait(false);
        }


        private async Task<ChannelItemResult> GetChannelItemsInternal(
            InternalChannelItemQuery query,
            CancellationToken cancellationToken)
        {
            var items = new List<ChannelItemInfo>();
            var playlists = Plugin.Instance.Configuration.M3UPlaylists;

            if (playlists == null)
                playlists = new List<M3UPlaylist>();

            // ROOT LEVEL: Show playlists as folders
            if (string.IsNullOrEmpty(query.FolderId))
            {
                foreach (var playlist in playlists)
                {
                    items.Add(new ChannelItemInfo
                    {
                        Name = playlist.Name,
                        Id = "folder-" + playlist.Name,
                        Type = ChannelItemType.Folder,
                        ImageUrl = playlist.Image
                    });
                }
            }

            // PLAYLIST LEVEL: Show channels inside playlist
            else if (query.FolderId.StartsWith("folder-"))
            {
                var playlistName = query.FolderId.Substring("folder-".Length);

                var playlist = playlists
                    .FirstOrDefault(p => p.Name.Equals(playlistName, StringComparison.OrdinalIgnoreCase));

                if (playlist != null && !string.IsNullOrEmpty(playlist.Path))
                {
                    string m3uContent;

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        m3uContent = await client
                            .GetStringAsync(playlist.Path)
                            .ConfigureAwait(false);
                    }

                    var parsedPlaylist = IPTVPlaylist.FromM3U(m3uContent, playlist.Name);

                    items.AddRange(parsedPlaylist.Items);
                }
            }

            return new ChannelItemResult
            {
                Items = items
            };
        }



        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Backdrop
            };
        }

        public string HomePageUrl
        {
            get { return ""; }
        }

        public string Name
        {
            get { return "IPTV"; }
        }

        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Clip
                },

                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                },

                SupportsContentDownloading = true
            };
        }

        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Thumb:
                case ImageType.Backdrop:
                    {
                        var path = GetType().Namespace + ".Images." + type.ToString().ToLower() + ".png";

                        return Task.FromResult(new DynamicImageResponse
                        {
                            Format = ImageFormat.Png,
                            Stream = GetType().Assembly.GetManifestResourceStream(path)
                        });
                    }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);
            }
        }

        public bool IsEnabledFor(string userId)
        {
            return true;
        }

        public ChannelParentalRating ParentalRating
        {
            get { return ChannelParentalRating.GeneralAudience; }
        }

        public string GetCacheKey(string userId)
        {
            return Guid.NewGuid().ToString("N");
        }

        public string Description
        {
            get { return string.Empty; }
        }

    }
}
