using System;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using System.Reflection;
using BilibiliMonitor;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool;
using Newtonsoft.Json.Linq;
using System.Linq;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp;
using System.IO;
using System.Text;
using BilibiliMonitor.Models;
using System.Collections.Generic;
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
            if(!Directory.Exists(Path.Combine(MainSave.AppDirectory, "Assets")))
            {
                MainSave.CQLog.Warning("资源文件不存在，请放置文件后重载插件");
                return;
            }
            UpdateChecker update = new(MainSave.AppDirectory, MainSave.ImageDirectory);
            LogHelper.InfoMethod = (type, message, status) =>
            {
                if(!status)
                {
                    MainSave.CQLog.Warning(type, message);
                }
                else
                {
                    MainSave.CQLog.Debug(type, message);
                }
            };
            MainSave.UpdateChecker = update;
            new Thread(() =>
            {
                update.DynamicCheckCD = 2;
                update.OnDynamic += UpdateChecker_OnDynamic;
                update.OnStream += UpdateChecker_OnStream;
                update.OnBangumi += UpdateChecker_OnBangumi;
                update.OnBangumiEnd += Update_OnBangumiEnd;
                JsonConfig.Init(MainSave.AppDirectory);
                var dynamics = JsonConfig.GetConfig<int[]>("Dynamics");
                var streams = JsonConfig.GetConfig<int[]>("Streams");
                var bangumis = JsonConfig.GetConfig<int[]>("Bangumis");
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
            var bangumis = JsonConfig.GetConfig<List<int>>("Bangumis");
            var group = JsonConfig.GetConfig<JObject>("Monitor_Bangumis");
            foreach (JProperty item in group.Properties())
            {
                if ((item.Value as JArray).Any(x => {
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
            var group = JsonConfig.GetConfig<JObject>("Monitor_Stream");
            foreach(JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if(o.Any(x=>x == userInfo.info.uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{userInfo.info.uname} 开播了, https://live.bilibili.com/{roomInfo.room_id}");
                    sb.Append(CQApi.CQCode_Image(picPath));
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), sb.ToString());
                }
            }
        }

        private void UpdateChecker_OnDynamic(BilibiliMonitor.Models.DynamicModel.Item item, int uid, string picPath)
        {
            var group = JsonConfig.GetConfig<JObject>("Monitor_Dynamic");
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if (o.Any(x => x == uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{item.modules.module_author.name} 更新了动态, https://t.bilibili.com/{item.id_str}");
                    sb.Append(CQApi.CQCode_Image(picPath));
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), sb.ToString());
                }
            }
        }
        private void UpdateChecker_OnBangumi(BangumiModel.DetailInfo bangumi, BangumiModel.Episode epInfo, string picPath)
        {
            var group = JsonConfig.GetConfig<JObject>("Monitor_Bangumis");
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if (o.Any(x => x == Convert.ToInt32(bangumi.result.season_id)))
                {
                    StringBuilder sb = new();
                    sb.Append($"{bangumi.result.title} 更新了新的一集, {epInfo.share_url}");
                    sb.Append(CQApi.CQCode_Image(picPath));
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), sb.ToString());
                }
            }
        }
    }
}
