using Another_Mirai_Native.Abstractions;
using Another_Mirai_Native.Abstractions.Attributes;
using Another_Mirai_Native.Abstractions.Models;
using Another_Mirai_Native.Abstractions.Models.MessageItem;
using BilibiliMonitor;
using BilibiliMonitor.BilibiliAPI;
using BilibiliMonitor.Models;
using System.Text;

namespace BilibiliUpdateCheckBot
{
    [PluginInfo("me.cqp.luohuaming.BiliBiliCheckBot", "", "2.0.0", "", "lhm")]
    public class Entry : PluginBase
    {
        public static Config Config { get; private set; }

        public override async Task OnEnableAsync(CancellationToken ct)
        {
            Config = new(Path.Combine(API.AppApi.GetAppDirectory(), "Config.json"));
            if (string.IsNullOrEmpty(BilibiliMonitor.Config.Cookies) || string.IsNullOrEmpty(BilibiliMonitor.Config.RefreshToken))
            {
                API.Logger.Warn("参数无效", "Cookie或RefreshToken无效");
                return;
            }
            if (!Directory.Exists(Path.Combine(API.AppApi.GetAppDirectory(), "Assets")))
            {
                API.Logger.Warn("缺少资源包", "资源文件不存在，请放置文件后重载插件");
                return;
            }
            if (Config == null)
            {
                API.Logger.Warn("配置加载", "配置文件加载失败，请检查是否配置文件格式");
                return;
            }
            Config.LoadConfig();
            BilibiliMonitor.Config.OnConfigSaveRequest += Config_OnConfigSaveRequest;
            LogHelper.InfoMethod = (type, message, status) =>
            {
                if (!status)
                {
                    API.Logger.Warn(type, message);
                }
                else
                {
                    API.Logger.Debug(type, message);
                }
            };
            await LoadMonitorItems();
            return;
        }

        private void Config_OnConfigSaveRequest()
        {
            Config.SaveConfig();
        }

        private async Task LoadMonitorItems()
        {
            Dynamics.OnDynamicUpdated += UpdateChecker_OnDynamic;
            LiveStreams.OnLiveStreamUpdated += UpdateChecker_OnStream;
            Bangumi.OnBangumiUpdated += UpdateChecker_OnBangumi;
            Bangumi.OnBangumiEnded += Update_OnBangumiEnd;

            await Task.Run(() =>
            {
                foreach (var item in Config.Dynamics)
                {
                    Dynamics.AddDynamic(item);
                }
                foreach (var item in Config.Streams)
                {
                    LiveStreams.AddStream(item);
                }
                foreach (var item in Config.Bangumis)
                {
                    Bangumi.AddBangumi(item);
                }
            });
            API.Logger.Info("载入成功", $"监视了 {Config.Dynamics.Count} 个动态，{Config.Streams.Count} 个直播，{Config.Bangumis.Count} 个番剧");

        }
        private void Update_OnBangumiEnd(Bangumi bangumi)
        {
            long sid = bangumi.SeasonID;
            var bangumis = Config.Bangumis;
            var group = Config.MonitorBangumis;
            foreach (var item in group)
            {
                item.TargetId.Remove(sid);
            }
            bangumis.Remove(sid);
            Config.Bangumis = bangumis;
            Config.MonitorBangumis = group;
            Config.SaveConfig();
        }

        private void UpdateChecker_OnStream(LiveStreamsModel.RoomInfo roomInfo, LiveStreamsModel.UserInfo userInfo, bool streaming, string info)
        {
            var group = Config.MonitorStreams;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == userInfo.info.uid))
                {
                    StringBuilder sb = new();
                    if (streaming)
                    {
                        sb.Append($"{userInfo.info.uname} 开播了, https://live.bilibili.com/{roomInfo.room_id}");
                        if (string.IsNullOrEmpty(info) is false)
                        {
                            sb.Append(new Image(info));
                        }
                        else
                        {
                            sb.Append($"\n{roomInfo.title} - {roomInfo.live_time}");
                        }
                    }
                    else
                    {
                        sb.Append($"{userInfo.info.uname} 下播了，{info}");
                    }
                    API.MessageApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnDynamic(DynamicModel.Item item, long uid, string picPath)
        {
            var group = Config.MonitorDynamics;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{item.modules.module_author.name} 更新了动态, https://t.bilibili.com/{item.id_str}");
                    if (string.IsNullOrEmpty(picPath) is false)
                    {
                        sb.Append(new Image(picPath));
                    }

                    API.MessageApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnBangumi(BangumiModel.DetailInfo bangumi, BangumiModel.Episode epInfo, string picPath)
        {
            var group = Config.MonitorBangumis;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == Convert.ToInt32(bangumi.result.season_id)))
                {
                    StringBuilder sb = new();
                    sb.Append($"{bangumi.result.title} 更新了新的一集, {epInfo.share_url}");
                    if (string.IsNullOrEmpty(picPath) is false)
                    {
                        sb.Append(new Image(picPath));
                    }
                    else
                    {
                        sb.Append($"\n{epInfo.long_title}");
                    }
                    API.MessageApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }
    }
}
