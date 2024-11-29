using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class RemoveStream : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#移除直播";

        public bool Judge(string destStr) => destStr.Replace("＃", "#").StartsWith(GetOrderStr());//这里判断是否能触发指令

        public FunctionResult Progress(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new()
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new()
            {
                SendID = e.FromGroup,
            };
            result.SendObject.Add(sendText);
            var args = e.Message.Text.Replace(GetOrderStr(), "");
            if (string.IsNullOrEmpty(args))
            {
                sendText.MsgToSend.Add("请填写用户ID或序号");
                return result;
            }
            if (!long.TryParse(args, out long uid))
            {
                sendText.MsgToSend.Add("用户ID或序号格式不正确");
                return result;
            }
            var streams = AppConfig.Instance.GetConfig<List<long>>("Streams", new());
            var group = AppConfig.Instance.GetConfig<JObject>("Monitor_Stream", new());
            if (group.ContainsKey(e.FromGroup))
            {
                var groupArr = group[e.FromGroup].ToObject<List<int>>();
                if (!groupArr.Any(x => x == uid))
                {
                    if (groupArr.Count > uid)
                    {
                        uid = groupArr[(int)uid - 1];
                    }
                    else
                    {
                        sendText.MsgToSend.Add("用户ID或序号格式不正确");
                        return result;
                    }
                }

                group[e.FromGroup].Children().FirstOrDefault(x => x.Value<int>() == uid)?.Remove();
            }
            AppConfig.Instance.SetConfig("Monitor_Stream", group);
            bool existFlag = false;
            foreach (JProperty item in group.Properties())
            {
                if ((item.Value as JArray).Any(x =>
                {
                    var p = (int)x;
                    return p == uid;
                }))
                {
                    existFlag = true;
                }
            }
            if (streams.Any(x => x == uid) && !existFlag)
            {
                streams.Remove(uid);
                MainSave.UpdateChecker.RemoveStream(uid);
                AppConfig.Instance.SetConfig("Streams", streams);
            }
            sendText.MsgToSend.Add("删除成功");
            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new()
            {
                Result = false,
                SendFlag = false,
            };
            SendText sendText = new()
            {
                SendID = e.FromQQ,
            };

            sendText.MsgToSend.Add("这里输入需要发送的文本");
            result.SendObject.Add(sendText);
            return result;
        }
    }
}