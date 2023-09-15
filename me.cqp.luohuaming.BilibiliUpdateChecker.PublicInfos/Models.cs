using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos
{
    public interface IOrderModel
    {
        bool ImplementFlag { get; set; }

        string GetOrderStr();

        bool Judge(string destStr);

        FunctionResult Progress(CQGroupMessageEventArgs e);

        FunctionResult Progress(CQPrivateMessageEventArgs e);
    }
}