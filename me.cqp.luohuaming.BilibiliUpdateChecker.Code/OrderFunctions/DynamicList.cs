using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code.OrderFunctions
{
    public class DynamicList : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#动态列表";

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

            StringBuilder sb = new();
            sb.AppendLine("动态列表");
            int index = 1;
            foreach (var item in MainSave.UpdateChecker.GetDynamicList())
            {
                var group = AppConfig.MonitorDynamics;
                var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup);
                if (groupItem != null && groupItem.TargetId.Any(x => x == item.Item1))
                {
                    sb.AppendLine($"{index}. {item.Item2} - {item.Item1}");
                    index++;
                }
            }
            sendText.MsgToSend.Add(sb.ToString()); result.SendObject.Add(sendText);
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