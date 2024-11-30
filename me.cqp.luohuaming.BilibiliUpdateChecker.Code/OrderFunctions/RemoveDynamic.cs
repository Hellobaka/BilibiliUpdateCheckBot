using BilibiliMonitor.BilibiliAPI;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class RemoveDynamic : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#移除动态";

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
            var dynamics = AppConfig.Dynamics;
            var group = AppConfig.MonitorDynamics;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup);
            if (groupItem != null)
            {
                if (!groupItem.TargetId.Contains(uid))
                {
                    if (groupItem.TargetId.Count > uid)
                    {
                        uid = groupItem.TargetId[(int)uid - 1];
                    }
                    else
                    {
                        sendText.MsgToSend.Add("用户ID或序号格式不正确");
                        return result;
                    }
                }

                groupItem.TargetId.Remove(uid);
            }
            AppConfig.Instance.SetConfig("MonitorDynamics", group);
            bool existFlag = false;
            foreach (var item in group)
            {
                if (item.TargetId.Contains(uid))
                {
                    existFlag = true;
                    break;
                }
            }
            if (dynamics.Any(x => x == uid) && !existFlag)
            {
                dynamics.Remove(uid);
                Dynamics.RemoveDynamic(uid);
                AppConfig.Instance.SetConfig("Dynamics", dynamics);
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
            return result;
        }
    }
}