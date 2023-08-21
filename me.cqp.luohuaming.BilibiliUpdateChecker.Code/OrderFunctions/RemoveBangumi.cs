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
    public class RemoveBangumi : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;
        
        public string GetOrderStr() => "#移除番剧";

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
                sendText.MsgToSend.Add("请填写番剧sid或序号");
                return result;
            }
            if (!int.TryParse(args, out int sid))
            {
                sendText.MsgToSend.Add("番剧sid或序号格式不正确");
                return result;
            }
            var bangumis = JsonConfig.GetConfig<List<int>>("Bangumis", new());
            
            var group = JsonConfig.GetConfig<JObject>("Monitor_Bangumis", new());
            if (group.ContainsKey(e.FromGroup))
            {
                var groupArr = group[e.FromGroup].ToObject<List<int>>();
                if (!groupArr.Any(x => x == sid))
                {
                    if (groupArr.Count >= sid)
                    {
                        sid = groupArr[sid - 1];
                    }
                    else
                    {
                        sendText.MsgToSend.Add("番剧sid或序号格式不正确");
                        return result;
                    }
                }
                group[e.FromGroup].Children().FirstOrDefault(x => x.Value<int>() == sid)?.Remove();
            }
            JsonConfig.WriteConfig("Monitor_Bangumis", group);
            bool existFlag = false;
            foreach(JProperty item in group.Properties())
            {
                if((item.Value as JArray).Any(x => {
                    var p = (int)x;
                    return p == sid;
                }))
                {
                    existFlag = true;
                }
            }
            if (bangumis.Any(x => x == sid) && !existFlag)
            {
                bangumis.Remove(sid);
                MainSave.UpdateChecker.RemoveBangumi(sid);
                JsonConfig.WriteConfig("Bangumis", bangumis);
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
