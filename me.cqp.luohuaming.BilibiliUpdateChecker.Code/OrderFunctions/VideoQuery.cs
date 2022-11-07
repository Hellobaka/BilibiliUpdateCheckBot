using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using BilibiliMonitor.BilibiliAPI;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp;
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

            string vid;
            if (e.Message.Text.Contains("[CQ:rich,") && e.Message.Text.Contains("b23.tv"))
            {
                vid = Videos.ParseURLFromXML(e.Message.Text);
            }
            else
            {
                string t = Regex.Replace(e.Message.Text, "\\[CQ:.*\\]", "");
                vid = Videos.ParseURL(t); 
            }
            if (string.IsNullOrEmpty(vid))
            {
                result.SendFlag = false;
                result.Result = false;
                return result;
            }
            try
            {
                sendText.MsgToSend.Add(CQApi.CQCode_Image(Videos.DrawVideoPic(vid)).ToString());
                if (e.Message.Text.Contains("b23.tv"))
                {
                    if(int.TryParse(vid, out int aid))
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
