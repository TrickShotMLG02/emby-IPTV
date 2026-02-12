using MediaBrowser.Model.Serialization;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Channels.IPTV
{
    public class LogoEntry
    {
        public string channel { get; set; }
        public string url { get; set; }
    }

    public static class LogoLoader
    {
        private const string LogosUrl = "https://iptv-org.github.io/api/logos.json";

        public static async Task<Dictionary<string, string>> LoadLogosAsync(IJsonSerializer jsonSerializer)
        {
            Dictionary<string, string> dict = null;
            HttpClient client = null;
            try
            {
                client = new HttpClient();
                var json = await client.GetStringAsync(LogosUrl);
                var logos = jsonSerializer.DeserializeFromString<List<LogoEntry>>(json);

                // Map channel name to URL
                dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (logos != null)
                {
                    foreach (var logo in logos)
                    {
                        if (!string.IsNullOrWhiteSpace(logo.channel) && !string.IsNullOrWhiteSpace(logo.url))
                        {
                            dict[logo.channel] = logo.url;
                        }
                    }
                }
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }

            return dict;
        }
    }
}
