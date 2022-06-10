using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool;
using Newtonsoft.Json.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class RemoveStream : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;
        
        public string GetOrderStr() => "#移除直播";

        public bool Judge(string destStr) => destStr.Replace("＃", "#").StartsWith(GetOrderStr());//这里判断是否能触发指令

        public FunctionResult Progress(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new SendText
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
            if (!int.TryParse(args, out int uid))
            {
                sendText.MsgToSend.Add("用户ID或序号格式不正确");
                return result;
            }
            var streams = JsonConfig.GetConfig<List<int>>("Streams");
            if (!streams.Any(x => x == uid))
            {
                if (streams.Count > uid)
                {
                    uid = streams[uid - 1];
                }
                else
                {
                    sendText.MsgToSend.Add("用户ID或序号格式不正确");
                    return result;
                }
            }
            var group = JsonConfig.GetConfig<JObject>("Monitor_Stream");
            if (group.ContainsKey(e.FromGroup))
            {
                (group[e.FromGroup] as JArray).Remove(uid);
            }
            JsonConfig.WriteConfig("Monitor_Streams", group);
            bool existFlag = false;
            foreach (JProperty item in group.Values())
            {
                if ((item.Value as JArray).Any(x => {
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
                JsonConfig.WriteConfig("Streams", streams);
            }
            sendText.MsgToSend.Add("删除成功");
            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = false,
                SendFlag = false,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromQQ,
            };

            sendText.MsgToSend.Add("这里输入需要发送的文本");
            result.SendObject.Add(sendText);
            return result;
        }
    }
}
