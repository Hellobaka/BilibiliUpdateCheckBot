using BilibiliMonitor.BilibiliAPI;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos.Models;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class AddStream : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#添加直播";

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
            var streams = AppConfig.Streams;
            var group = AppConfig.MonitorStreams;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup);
            if (groupItem != null)
            {
                if (groupItem.TargetId.Contains(uid) && streams.Any(x => x == e.FromGroup))
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
            AppConfig.Instance.SetConfig("MonitorStreams", group);
            LiveStreams live = null;
            if (!streams.Any(x => x == uid))
            {
                streams.Add(uid);
                live = LiveStreams.AddStream(uid);
                AppConfig.Instance.SetConfig("Streams", streams);
            }
            if (live != null)
            {
                sendText.MsgToSend.Add($"{live.Name} 添加直播监视成功");
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