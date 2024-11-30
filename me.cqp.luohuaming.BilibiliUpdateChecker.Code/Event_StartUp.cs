using BilibiliMonitor;
using BilibiliMonitor.BilibiliAPI;
using BilibiliMonitor.Models;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code
{
    public class Event_StartUp : ICQStartup
    {
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MainSave.AppDirectory = e.CQApi.AppDirectory;
            MainSave.CQApi = e.CQApi;
            MainSave.CQLog = e.CQLog;
            MainSave.ImageDirectory = CommonHelper.GetAppImageDirectory();
            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();
            Config.Instance = appConfig;
            //这里写处理逻辑
            foreach (var item in Assembly.GetAssembly(typeof(Event_GroupMessage)).GetTypes())
            {
                if (item.IsInterface)
                {
                    continue;
                }

                foreach (var instance in item.GetInterfaces())
                {
                    if (instance == typeof(IOrderModel))
                    {
                        IOrderModel obj = (IOrderModel)Activator.CreateInstance(item);
                        if (obj.ImplementFlag == false)
                        {
                            break;
                        }

                        MainSave.Instances.Add(obj);
                    }
                }
            }
            if (!Directory.Exists(Path.Combine(MainSave.AppDirectory, "Assets")))
            {
                MainSave.CQLog.Warning("资源文件不存在，请放置文件后重载插件");
                return;
            }
            Config.BaseDirectory = MainSave.AppDirectory;
            Config.PicSaveBasePath = MainSave.ImageDirectory;
            LogHelper.InfoMethod = (type, message, status) =>
            {
                if (!status)
                {
                    MainSave.CQLog.Warning(type, message);
                }
                else
                {
                    MainSave.CQLog.Debug(type, message);
                }
            };
            new Thread(() =>
            {
                Dynamics.OnDynamicUpdated += UpdateChecker_OnDynamic;
                LiveStreams.OnLiveStreamUpdated += UpdateChecker_OnStream;
                Bangumi.OnBanguimiUpdated += UpdateChecker_OnBangumi;
                Bangumi.OnBanguimiEnded += Update_OnBangumiEnd;
                var dynamics = AppConfig.Dynamics;
                var streams = AppConfig.Streams;
                var bangumis = AppConfig.Bangumis;
                foreach (var item in dynamics)
                {
                    Dynamics.AddDynamic(item);
                }
                foreach (var item in streams)
                {
                    LiveStreams.AddStream(item);
                }
                foreach (var item in bangumis)
                {
                    Bangumi.AddBangumi(item);
                }
                MainSave.CQLog.Info("载入成功", $"监视了 {dynamics.Count} 个动态，{streams.Count} 个直播，{bangumis.Count} 个番剧");
            }).Start();
        }

        private void Update_OnBangumiEnd(BilibiliMonitor.BilibiliAPI.Bangumi bangumi)
        {
            long sid = bangumi.SeasonID;
            var bangumis = AppConfig.Bangumis;
            var group = AppConfig.MonitorBangumis;
            foreach (var item in group)
            {
                item.TargetId.Remove(sid);
            }
            bangumis.Remove(sid);
            AppConfig.Instance.SetConfig("Bangumis", bangumis);
            AppConfig.Instance.SetConfig("MonitorBangumis", group);
        }

        private void UpdateChecker_OnStream(LiveStreamsModel.RoomInfo roomInfo, LiveStreamsModel.UserInfo userInfo, string picPath)
        {
            var group = AppConfig.MonitorStreams;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == userInfo.info.uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{userInfo.info.uname} 开播了, https://live.bilibili.com/{roomInfo.room_id}");
                    if (string.IsNullOrEmpty(picPath) is false)
                    {
                        sb.Append(CQApi.CQCode_Image(picPath));
                    }
                    else
                    {
                        sb.Append($"\n{roomInfo.title} - {roomInfo.live_time}");
                    }
                    MainSave.CQApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnDynamic(DynamicModel.Item item, long uid, string picPath)
        {
            var group = AppConfig.MonitorDynamics;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{item.modules.module_author.name} 更新了动态, https://t.bilibili.com/{item.id_str}");
                    if (string.IsNullOrEmpty(picPath) is false)
                    {
                        sb.Append(CQApi.CQCode_Image(picPath));
                    }

                    MainSave.CQApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnBangumi(BangumiModel.DetailInfo bangumi, BangumiModel.Episode epInfo, string picPath)
        {
            var group = AppConfig.MonitorBangumis;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == Convert.ToInt32(bangumi.result.season_id)))
                {
                    StringBuilder sb = new();
                    sb.Append($"{bangumi.result.title} 更新了新的一集, {epInfo.share_url}");
                    if (string.IsNullOrEmpty(picPath) is false)
                    {
                        sb.Append(CQApi.CQCode_Image(picPath));
                    }
                    else
                    {
                        sb.Append($"\n{epInfo.long_title}");
                    }
                    MainSave.CQApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }
    }
}