using BilibiliMonitor;
using BilibiliMonitor.Models;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool;
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
            JsonConfig.Init(MainSave.AppDirectory);

            //这里写处理逻辑
            foreach (var item in Assembly.GetAssembly(typeof(Event_GroupMessage)).GetTypes())
            {
                if (item.IsInterface)
                    continue;
                foreach (var instance in item.GetInterfaces())
                {
                    if (instance == typeof(IOrderModel))
                    {
                        IOrderModel obj = (IOrderModel)Activator.CreateInstance(item);
                        if (obj.ImplementFlag == false)
                            break;
                        MainSave.Instances.Add(obj);
                    }
                }
            }
            if (!Directory.Exists(Path.Combine(MainSave.AppDirectory, "Assets")))
            {
                MainSave.CQLog.Warning("资源文件不存在，请放置文件后重载插件");
                return;
            }
            UpdateChecker update = new(MainSave.AppDirectory, MainSave.ImageDirectory);
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
            MainSave.UpdateChecker = update;
            MainSave.UpdateChecker.DebugMode = JsonConfig.GetConfig("DebugMode", false);
            new Thread(() =>
            {
                update.DynamicCheckCD = 2;
                update.OnDynamic += UpdateChecker_OnDynamic;
                update.OnStream += UpdateChecker_OnStream;
                update.OnBangumi += UpdateChecker_OnBangumi;
                update.OnBangumiEnd += Update_OnBangumiEnd;
                CommonHelper.UpdateCookie();
                var dynamics = JsonConfig.GetConfig<long[]>("Dynamics", new long[] { });
                var streams = JsonConfig.GetConfig<long[]>("Streams", new long[] { });
                var bangumis = JsonConfig.GetConfig<int[]>("Bangumis", new int[] { });
                foreach (var item in dynamics)
                {
                    update.AddDynamic(item);
                }
                foreach (var item in streams)
                {
                    update.AddStream(item);
                }
                foreach (var item in bangumis)
                {
                    update.AddBangumi(item);
                }
                update.Start();
                MainSave.CQLog.Info("载入成功", $"监视了 {dynamics.Length} 个动态，{streams.Length} 个直播，{bangumis.Length} 个番剧");
            }).Start();
        }

        private void Update_OnBangumiEnd(BilibiliMonitor.BilibiliAPI.Bangumi bangumi)
        {
            int sid = bangumi.SeasonID;
            var bangumis = JsonConfig.GetConfig<List<int>>("Bangumis", new());
            var group = JsonConfig.GetConfig<JObject>("Monitor_Bangumis", new());
            foreach (JProperty item in group.Properties())
            {
                if ((item.Value as JArray).Any(x =>
                {
                    var p = (int)x;
                    return p == sid;
                }))
                {
                    item.Value.Children().FirstOrDefault(x => x.Value<int>() == sid)?.Remove();
                }
            }
            bangumis.Remove(sid);
            JsonConfig.WriteConfig("Bangumis", bangumis);
            JsonConfig.WriteConfig("Monitor_Bangumis", group);
        }

        private void UpdateChecker_OnStream(LiveStreamsModel.RoomInfo roomInfo, LiveStreamsModel.UserInfo userInfo, string picPath)
        {
            var group = JsonConfig.GetConfig<JObject>("Monitor_Stream", new());
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<long[]>();
                if (o.Any(x => x == userInfo.info.uid))
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
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnDynamic(DynamicModel.Item item, long uid, string picPath)
        {
            var group = JsonConfig.GetConfig<JObject>("Monitor_Dynamic", new());
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<long[]>();
                if (o.Any(x => x == uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{item.modules.module_author.name} 更新了动态, https://t.bilibili.com/{item.id_str}");
                    if (string.IsNullOrEmpty(picPath) is false)
                        sb.Append(CQApi.CQCode_Image(picPath));
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), sb.ToString());
                }
            }
        }
        private void UpdateChecker_OnBangumi(BangumiModel.DetailInfo bangumi, BangumiModel.Episode epInfo, string picPath)
        {
            var group = JsonConfig.GetConfig<JObject>("Monitor_Bangumis", new());
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if (o.Any(x => x == Convert.ToInt32(bangumi.result.season_id)))
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
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), sb.ToString());
                }
            }
        }
    }
}
