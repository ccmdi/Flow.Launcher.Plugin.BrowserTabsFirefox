using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.ShimTabs
{
    public class Main : IAsyncPlugin
    {
        private PluginInitContext _context;
        private const string Host = "127.0.0.1";
        private const int Port = 19876;

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var tabs = await FetchTabsAsync();
            
            return tabs
                .Select(t => new
                {
                    Tab = t,
                    TitleMatch = _context.API.FuzzySearch(query.Search, t.Title),
                    UrlMatch = _context.API.FuzzySearch(query.Search, t.Url)
                })
                .Where(x => string.IsNullOrWhiteSpace(query.Search) || 
                            x.TitleMatch.Score > 0 || 
                            x.UrlMatch.Score > 0)
                .OrderByDescending(x => Math.Max(x.TitleMatch.Score, x.UrlMatch.Score))
                .Select(x => new Result
                {
                    Title = x.Tab.Title,
                    SubTitle = x.Tab.Url,
                    IcoPath = "Images/icon.png",
                    TitleHighlightData = x.TitleMatch.MatchData,
                    Action = _ =>
                    {
                        SendCommand("switch_tab", x.Tab.Id, x.Tab.WindowId);
                        return true;
                    }
                })
                .ToList();
        }

        private async Task<List<ShimTab>> FetchTabsAsync()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(Host, Port);
                using var stream = client.GetStream();

                var cmd = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { action = "get_tabs" }));
                await stream.WriteAsync(cmd, 0, cmd.Length);

                var buffer = new byte[128 * 1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                return JsonSerializer.Deserialize<List<ShimTab>>(json) ?? new List<ShimTab>();
            }
            catch
            {
                return new List<ShimTab>();
            }
        }

        private void SendCommand(string action, long tabId, long windowId)
        {
            try
            {
                using var client = new TcpClient(Host, Port);
                using var stream = client.GetStream();
                var cmd = JsonSerializer.Serialize(new { action, tabId, windowId });
                var data = Encoding.UTF8.GetBytes(cmd);
                stream.Write(data, 0, data.Length);
            }
            catch { }
        }
    }

    public class ShimTab
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("windowId")] public long WindowId { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; }
    }
}