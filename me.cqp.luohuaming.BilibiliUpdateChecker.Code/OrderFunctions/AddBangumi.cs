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
    public class AddBangumi : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#添加番剧";

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
                sendText.MsgToSend.Add("请填写番剧sid");
                return result;
            }
            if (!int.TryParse(args, out int sid))
            {
                sendText.MsgToSend.Add("番剧sid格式不正确");
                return result;
            }
            var bangumisList = AppConfig.Bangumis;
            var group = AppConfig.MonitorBangumis;
            Bangumi ban = null;
            if (!bangumisList.Any(x => x == sid))
            {
                bangumisList.Add(sid);
                ban = Bangumi.AddBangumi(sid);
            }
            var groupItem = group.FirstOrDefault(x=>x.GroupId == e.FromGroup);
            if (groupItem != null)
            {
                if (groupItem.TargetId.Contains(sid) && bangumisList.Any(x => x == e.FromGroup))
                {
                    sendText.MsgToSend.Add("重复添加");
                    return result;
                }
                groupItem.TargetId.Add(sid);
            }
            else
            {
                group.Add(new MonitorItem { GroupId = e.FromGroup, TargetId = [sid] });
            }
            if (ban != null)
            {
                AppConfig.Instance.SetConfig("Bangumis", bangumisList);
                AppConfig.Instance.SetConfig("MonitorBangumis", group);

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
            FunctionResult result = new()
            {
                Result = false,
                SendFlag = false,
            };
            return result;
        }
    }
}