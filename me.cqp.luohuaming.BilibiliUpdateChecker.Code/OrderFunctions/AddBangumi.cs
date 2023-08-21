using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BilibiliMonitor.BilibiliAPI;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool;
using Newtonsoft.Json.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class AddBangumi : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;
        
        public string GetOrderStr() => "#添加番剧";

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
            if(string.IsNullOrEmpty(args))
            {
                sendText.MsgToSend.Add("请填写番剧sid");
                return result;
            }
            if(!int.TryParse(args, out int sid))
            {
                sendText.MsgToSend.Add("番剧sid格式不正确");
                return result;
            }
            var bangumisList = JsonConfig.GetConfig<List<int>>("Bangumis", new());
            var group = JsonConfig.GetConfig<JObject>("Monitor_Bangumis", new());
            Bangumi ban = null;
            if(!bangumisList.Any(x=>x == sid))
            {
                bangumisList.Add(sid);
                ban = MainSave.UpdateChecker.AddBangumi(sid);
            }
            if (group.ContainsKey(e.FromGroup))
            {
                if (group[e.FromGroup].Contains(sid) && bangumisList.Any(x => x == e.FromGroup))
                {
                    sendText.MsgToSend.Add("重复添加");
                    return result;
                }
                (group[e.FromGroup] as JArray).Add(sid);
            }
            else
            {
                group.Add(new JProperty(e.FromGroup, new JArray(sid)));
            }
            if (ban != null)
            {
                JsonConfig.WriteConfig("Bangumis", bangumisList);
                JsonConfig.WriteConfig("Monitor_Bangumis", group);

                sendText.MsgToSend.Add($"{ban.Name} 添加番剧更新检查成功");
            }
            else
            {
                sendText.MsgToSend.Add("添加失败，可能番剧不存在或非支持地区番剧");
            }

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
