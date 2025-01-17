using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code
{
    public class Event_GroupMessage
    {
        public static FunctionResult GroupMessage(CQGroupMessageEventArgs e)
        {
            FunctionResult result = new()
            {
                SendFlag = false
            };
            try
            {
                if (BlockerHandler(e.FromGroup) is false)
                {
                    return result;
                }
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

        public static bool BlockerHandler(long group)
        {
            return AppConfig.FilterType switch
            {
                FilterType.WhiteList => AppConfig.WhiteList.Any(x => x == group),
                FilterType.BlackList => !AppConfig.BlackList.Any(x => x == group),
                _ => false,
            };
        }
    }
}