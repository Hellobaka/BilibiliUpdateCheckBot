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
    public class AddStream : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;
        
        public string GetOrderStr() => "#添加直播";

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
                sendText.MsgToSend.Add("请填写用户ID");
                return result;
            }
            if (!int.TryParse(args, out int uid))
            {
                sendText.MsgToSend.Add("用户ID格式不正确");
                return result;
            }
            var streams = JsonConfig.GetConfig<List<int>>("Streams");
            var group = JsonConfig.GetConfig<JObject>("Monitor_Streams");
            if (group.ContainsKey(e.FromGroup))
            {
                (group[e.FromGroup] as JArray).Add(uid);
            }
            else
            {
                group.Add(new JProperty(e.FromGroup, new JArray(uid)));
            }
            JsonConfig.WriteConfig("Monitor_Streams", group);
            if (!streams.Any(x => x == uid))
            {
                streams.Add(uid);
                MainSave.UpdateChecker.AddStream(uid);
                JsonConfig.WriteConfig("Streams", streams);
            }
            sendText.MsgToSend.Add("添加成功"); 
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
