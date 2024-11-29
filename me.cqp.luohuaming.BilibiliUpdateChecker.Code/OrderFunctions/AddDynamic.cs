using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class AddDynamic : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#添加动态";

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
                sendText.MsgToSend.Add("请填写用户ID");
                return result;
            }
            if (!long.TryParse(args, out long uid))
            {
                sendText.MsgToSend.Add("用户ID格式不正确");
                return result;
            }
            var dynamicsList = AppConfig.Instance.GetConfig<List<long>>("Dynamics", new());
            var group = AppConfig.Instance.GetConfig<JObject>("Monitor_Dynamic", new());
            if (group.ContainsKey(e.FromGroup))
            {
                if (group[e.FromGroup].Contains(uid) && dynamicsList.Any(x => x == e.FromGroup))
                {
                    sendText.MsgToSend.Add("重复添加");
                    return result;
                }

                (group[e.FromGroup] as JArray).Add(uid);
            }
            else
            {
                group.Add(new JProperty(e.FromGroup, new JArray(uid)));
            }
            AppConfig.Instance.SetConfig("Monitor_Dynamic", group);
            if (!dynamicsList.Any(x => x == uid))
            {
                dynamicsList.Add(uid);
                var dy = MainSave.UpdateChecker.AddDynamic(uid);
                AppConfig.Instance.SetConfig("Dynamics", dynamicsList);
            }
            var c = MainSave.UpdateChecker.GetDynamic(uid);
            if (c != null)
            {
                sendText.MsgToSend.Add($"{c.UserName} 添加动态监视成功");
            }
            else
            {
                sendText.MsgToSend.Add("添加失败");
            }

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