using BilibiliUpdateCheckBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliUpdateCheckBot
{
    public class Config
    {
        private readonly Lock SyncLock = new();

        private string ConfigPath { get; set; }

        private JObject? ConfigBody { get; set; }

        public FilterType FilterType { get; set; }

        public List<long> WhiteList { get; set; } = [];

        public List<long> BlackList { get; set; } = [];

        public List<long> Dynamics { get; set; } = [];

        public List<long> Streams { get; set; } = [];

        public List<long> Bangumis { get; set; } = [];

        public List<MonitorItem> MonitorDynamics { get; set; } = [];

        public List<MonitorItem> MonitorStreams { get; set; } = [];

        public List<MonitorItem> MonitorBangumis { get; set; } = [];

        public Config(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }
            ConfigPath = configPath;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? "");
                if (File.Exists(ConfigPath) is false)
                {
                    File.WriteAllText(ConfigPath, "{}");
                }
                ConfigBody = JObject.Parse(File.ReadAllText(ConfigPath));
            }
            catch
            {
                // load failed
            }
        }

        public void LoadConfig()
        {
            FilterType = ReadKey("FilterType", FilterType.WhiteList);
            WhiteList = ReadKey("WhiteList", new List<long>());
            BlackList = ReadKey("BlackList", new List<long>());
            Dynamics = ReadKey("Dynamics", new List<long>());
            Streams = ReadKey("Streams", new List<long>());
            Bangumis = ReadKey("Bangumis", new List<long>());
            MonitorDynamics = ReadKey("MonitorDynamics", new List<MonitorItem>());
            MonitorStreams = ReadKey("MonitorStreams", new List<MonitorItem>());
            MonitorBangumis = ReadKey("MonitorBangumis", new List<MonitorItem>());

            BilibiliMonitor.Config.Cookies = ReadKey("Cookies", "");
            BilibiliMonitor.Config.RefreshToken = ReadKey("RefreshToken", "");
            BilibiliMonitor.Config.CustomFont = ReadKey("CustomFont", "Microsoft YaHei");
            BilibiliMonitor.Config.CustomFontPath = ReadKey("CustomFontPath", "");
            BilibiliMonitor.Config.RefreshInterval = ReadKey("RefreshInterval", 120 * 1000);
            BilibiliMonitor.Config.BangumiRetryCount = ReadKey("BangumiRetryCount", 3);
            BilibiliMonitor.Config.LiveStreamRetryCount = ReadKey("LiveStreamRetryCount", 3);
            BilibiliMonitor.Config.DynamicRetryCount = ReadKey("DynamicRetryCount", 3);
            BilibiliMonitor.Config.DebugMode = ReadKey("DebugMode", false);
            BilibiliMonitor.Config.DynamicFilters = ReadKey("DynamicFilters", new List<string>() { "UP主的推荐", "ADDITIONAL_TYPE_GOODS" });
        }

        public void SaveConfig()
        {
            if (ConfigBody == null)
            {
                return;
            }
            File.WriteAllText(ConfigPath, ConfigBody.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        private T ReadKey<T>(string key, T defaultValue)
        {
            lock (SyncLock)
            {
                if (ConfigBody == null)
                {
                    CreateDefaultConfig();
                }
                if (ConfigBody!.TryGetValue(key, out var existValue))
                {
                    return existValue.ToObject<T>() ?? defaultValue;
                }
                ConfigBody[key] = JToken.FromObject(defaultValue);
                SaveConfig();
            }
            return defaultValue;
        }

        private void CreateDefaultConfig()
        {
            ConfigBody = new JObject();
            SaveConfig();
        }
    }
}
