using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        // TODO: 移动配置至此类中
        // TODO: Mode改为枚举类型
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

        public static bool DebugMode { get; private set; }

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
            DebugMode = GetConfig("DebugMode", false);
            Dynamics = GetConfig("Dynamics", new List<long>());
            Streams = GetConfig("Streams", new List<long>());
            Bangumis = GetConfig("Bangumis", new List<long>());
            MonitorDynamics = GetConfig("MonitorDynamics", new List<MonitorItem>());
            MonitorStreams = GetConfig("MonitorStreams", new List<MonitorItem>());
            MonitorBangumis = GetConfig("MonitorBangumis", new List<MonitorItem>());
        }
    }
}
