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
            MainSave.UpdateChecker = update;
            update.DynamicCheckCD = 2;
            update.OnDynamic += UpdateChecker_OnDynamic;
            update.OnStream += UpdateChecker_OnStream;
            JsonConfig.Init(MainSave.AppDirectory);
            var dynamics = JsonConfig.GetConfig<int[]>("Dynamics");
            var streams = JsonConfig.GetConfig<int[]>("Streams");
            foreach (var item in dynamics)
            {
                update.AddDynamic(item);
            }
            foreach (var item in streams)
            {
                update.AddStream(item);
            }
            update.Start();
        }

        private void UpdateChecker_OnStream(BilibiliMonitor.Models.LiveStreamsModel.RoomInfo item, string picPath)
        {
            var group = JsonConfig.GetConfig<JObject>("Monitor_Stream");
            foreach(JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if(o.Any(x=>x == item.uid))
                {
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), CQApi.CQCode_Image(picPath));
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
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id.Name), CQApi.CQCode_Image(picPath));
                }
            }
        }
    }
}
