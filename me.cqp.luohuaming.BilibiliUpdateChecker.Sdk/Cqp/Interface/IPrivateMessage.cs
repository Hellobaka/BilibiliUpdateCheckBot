﻿using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface
{
    /// <summary>
    /// 酷Q私聊消息事件接口
    /// <para/>
    /// Type: 21
    /// </summary>
    public interface IPrivateMessage
    {
        /// <summary>
        /// 当在派生类中重写时, 处理 酷Q私聊消息事件 回调
        /// </summary>
        /// <param name="sender">事件来源对象</param>
        /// <param name="e">附加的事件参数</param>
        void PrivateMessage(object sender, CQPrivateMessageEventArgs e);
    }
}
