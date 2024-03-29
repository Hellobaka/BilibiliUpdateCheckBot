﻿using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface
{
    /// <summary>
    /// 酷Q好友添加请求事件接口
    /// <para/>
    /// Type: 301
    /// </summary>
    public interface IFriendAddRequest
    {
        /// <summary>
        /// 当在派生类中重写时, 处理 酷Q好友添加请求事件 回调
        /// </summary>
        /// <param name="sender">事件来源对象</param>
        /// <param name="e">附加的事件参数</param>
        void FriendAddRequest(object sender, CQFriendAddRequestEventArgs e);
    }
}
