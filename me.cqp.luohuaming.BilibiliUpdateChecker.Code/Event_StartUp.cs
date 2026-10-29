using BilibiliMonitor;
using BilibiliMonitor.BilibiliAPI;
using BilibiliMonitor.Models;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface;
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
        private SentDynamicStore SentDynamics { get; set; }

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
            if (string.IsNullOrEmpty(Config.Cookies) || string.IsNullOrEmpty(Config.RefreshToken))
            {
                MainSave.CQLog.Warning("Cookie或RefreshToken无效");
                return;
            }
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
                            continue;
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
                try
                {
                    SentDynamics = new SentDynamicStore(Path.Combine(MainSave.AppDirectory, "SentDynamics.json"));
                    Dynamics.OnDynamicUpdated += UpdateChecker_OnDynamic;
                    foreach (var item in AppConfig.Dynamics)
                    {
                        Dynamics.AddDynamic(item);
                    }
                }
                catch (Exception exc)
                {
                    MainSave.CQLog.Warning("动态监视启动失败", exc.Message);
                }
                LiveStreams.OnLiveStreamUpdated += UpdateChecker_OnStream;
                Bangumi.OnBangumiUpdated += UpdateChecker_OnBangumi;
                Bangumi.OnBangumiEnded += Update_OnBangumiEnd;

                foreach (var item in AppConfig.Streams)
                {
                    LiveStreams.AddStream(item);
                }
                foreach (var item in AppConfig.Bangumis)
                {
                    Bangumi.AddBangumi(item);
                }
                MainSave.CQLog.Info("载入成功", $"监视了 {AppConfig.Dynamics.Count} 个动态，{AppConfig.Streams.Count} 个直播，{AppConfig.Bangumis.Count} 个番剧");
            }).Start();
        }

        private void Update_OnBangumiEnd(Bangumi bangumi)
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

        private void UpdateChecker_OnStream(LiveStreamsModel.RoomInfo roomInfo, LiveStreamsModel.UserInfo userInfo, bool streaming, string info)
        {
            var group = AppConfig.MonitorStreams;
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
                            sb.Append(CQApi.CQCode_Image(info));
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
                    MainSave.CQApi.SendGroupMessage(id.GroupId, sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnDynamic(DynamicModel.Item item, long uid, string picPath)
        {
            List<Exception> failures = [];
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

                    try
                    {
                        SentDynamics.SendOnce(id.GroupId, item.id_str,
                            () => MainSave.CQApi.SendGroupMessage(id.GroupId, sb.ToString()).IsSuccess);
                    }
                    catch (Exception exc)
                    {
                        failures.Add(exc);
                    }
                }
            }
            if (failures.Count > 0)
            {
                throw new AggregateException("部分群动态发送或记录保存失败", failures);
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
