using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code
{
    public class Event_GroupMessage
    {
        public static FunctionResult GroupMessage(CQGroupMessageEventArgs e)
        {
            FunctionResult result = new FunctionResult()
            {
                SendFlag = false
            };
            try
            {
                if(BlockerHandler(e.FromGroup) is false)
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
            int mode = JsonConfig.GetConfig<int>("Mode");
            List<long> ls = null;
            switch (mode)
            {
                case 0:
                    ls = JsonConfig.GetConfig<List<long>>("WhiteList");
                    return ls.Any(x => x == group);
                case 1:
                    ls = JsonConfig.GetConfig<List<long>>("BlackList");
                    return ls.Any(x => x != group);
                default:
                    break;
            }
            return false;
        }
    }
}
