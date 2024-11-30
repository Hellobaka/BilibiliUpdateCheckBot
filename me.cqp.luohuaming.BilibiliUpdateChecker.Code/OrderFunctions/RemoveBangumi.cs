using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class RemoveBangumi : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#移除番剧";

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
                sendText.MsgToSend.Add("请填写番剧sid或序号");
                return result;
            }
            if (!long.TryParse(args, out long sid))
            {
                sendText.MsgToSend.Add("番剧sid或序号格式不正确");
                return result;
            }
            var bangumis = AppConfig.Bangumis;
            var group = AppConfig.MonitorBangumis;

            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup);
            if (groupItem != null)
            {
                if (!groupItem.TargetId.Contains(sid))
                {
                    if (groupItem.TargetId.Count >= sid)
                    {
                        sid = groupItem.TargetId[(int)(sid - 1)];
                    }
                    else
                    {
                        sendText.MsgToSend.Add("番剧sid或序号格式不正确");
                        return result;
                    }
                }
                groupItem.TargetId.Remove(sid);
            }
            AppConfig.Instance.SetConfig("MonitorBangumis", group);
            bool existFlag = false;
            foreach (var item in group)
            {
                if (item.TargetId.Contains(sid))
                {
                    existFlag = true;
                    break;
                }
            }
            if (bangumis.Any(x => x == sid) && !existFlag)
            {
                bangumis.Remove(sid);
                MainSave.UpdateChecker.RemoveBangumi(sid);
                AppConfig.Instance.SetConfig("Bangumis", bangumis);
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