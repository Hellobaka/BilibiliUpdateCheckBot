using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using System;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code
{
    public class Event_PrivateMessage
    {
        public static FunctionResult PrivateMessage(CQPrivateMessageEventArgs e)
        {
            FunctionResult result = new()
            {
                SendFlag = false
            };
            try
            {
                foreach (var item in MainSave.Instances.Where(item => item.Judge(e.Message.Text)))
                {
                    return item.Progress(e);
                }
                return result;
            }
            catch (Exception exc)
            {
                MainSave.CQLog.Info("异常抛出", exc.Message + exc.StackTrace);
                return result;
            }
        }
    }
}