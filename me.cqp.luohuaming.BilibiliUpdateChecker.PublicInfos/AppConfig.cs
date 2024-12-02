using BilibiliMonitor;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos.Models;
using System.Collections.Generic;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        public AppConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static AppConfig Instance { get; private set; }

        public static FilterType FilterType { get; private set; }

        public static List<long> WhiteList { get; private set; } = [];

        public static List<long> BlackList { get; private set; } = [];

        public static List<long> Dynamics { get; private set; } = [];

        public static List<long> Streams { get; private set; } = [];

        public static List<long> Bangumis { get; private set; } = [];

        public static List<MonitorItem> MonitorDynamics { get; private set; } = [];

        public static List<MonitorItem> MonitorStreams { get; private set; } = [];

        public static List<MonitorItem> MonitorBangumis { get; private set; } = [];

        public override void LoadConfig()
        {
            FilterType = GetConfig("FilterType", FilterType.WhiteList);
            WhiteList = GetConfig("WhiteList", new List<long>());
            BlackList = GetConfig("BlackList", new List<long>());
            Dynamics = GetConfig("Dynamics", new List<long>());
            Streams = GetConfig("Streams", new List<long>());
            Bangumis = GetConfig("Bangumis", new List<long>());
            MonitorDynamics = GetConfig("MonitorDynamics", new List<MonitorItem>());
            MonitorStreams = GetConfig("MonitorStreams", new List<MonitorItem>());
            MonitorBangumis = GetConfig("MonitorBangumis", new List<MonitorItem>());

            Config.Cookies = GetConfig("Cookies", "");
            Config.RefreshToken = GetConfig("RefreshToken", "");
            Config.RefreshInterval = GetConfig("RefreshInterval", 120 * 1000);
            Config.BangumiRetryCount = GetConfig("BangumiRetryCount", 3);
            Config.DynamicRetryCount = GetConfig("DynamicRetryCount", 3);
            Config.LiveStreamRetryCount = GetConfig("LiveStreamRetryCount", 3);
            Config.DebugMode = GetConfig("DebugMode", false);
            Config.CustomFont = GetConfig("CustomFont", "Microsoft YaHei");
            Config.CustomFontPath = GetConfig("CustomFontPath", "");
            Config.DynamicFilters = GetConfig("DynamicFilters", new List<string>() { "UP主的推荐", "ADDITIONAL_TYPE_GOODS" });
        }
    }
}
