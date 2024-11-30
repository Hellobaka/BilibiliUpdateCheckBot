using BilibiliMonitor.BilibiliAPI;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos.Models;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

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
            var dynamicsList = AppConfig.Dynamics;
            var group = AppConfig.MonitorDynamics;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup);
            if (groupItem != null)
            {
                if (groupItem.TargetId.Contains(uid) && dynamicsList.Any(x => x == e.FromGroup))
                {
                    sendText.MsgToSend.Add("重复添加");
                    return result;
                }

                groupItem.TargetId.Add(uid);
            }
            else
            {
                group.Add(new MonitorItem { GroupId = e.FromGroup, TargetId = [uid] });
            }
            AppConfig.Instance.SetConfig("MonitorDynamics", group);
            if (!dynamicsList.Any(x => x == uid))
            {
                dynamicsList.Add(uid);
                var dy = Dynamics.AddDynamic(uid);
                AppConfig.Instance.SetConfig("Dynamics", dynamicsList);
            }
            var c = Dynamics.GetDynamic(uid);
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
            return result;
        }
    }
}