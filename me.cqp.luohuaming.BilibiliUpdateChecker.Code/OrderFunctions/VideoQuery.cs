using BilibiliMonitor.BilibiliAPI;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using System.Text.RegularExpressions;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class VideoQuery : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#解析";

        public bool Judge(string destStr)
        {
            return true;
        }

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

            string vid = Videos.ParseURL(e.Message.Text);
            if (string.IsNullOrEmpty(vid))
            {
                result.SendFlag = false;
                result.Result = false;
                return result;
            }
            try
            {
                sendText.MsgToSend.Add(CQApi.CQCode_Image(Videos.DrawVideoPic(vid)).ToString());
                if (e.Message.Text.Contains("b23.tv") || e.Message.Text.Contains("bili2233.xn"))
                {
                    if (int.TryParse(vid, out int aid))
                    {
                        sendText.MsgToSend.Add($"https://www.bilibili.com/video/av{aid}");
                    }
                    else
                    {
                        sendText.MsgToSend.Add($"https://www.bilibili.com/video/{vid}");
                    }
                }
            }
            catch
            {
                result.SendFlag = false;
                result.Result = false;
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