﻿using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Model;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Interface
{
    /// <summary>
    /// 酷Q更新状态事件接口
    /// </summary>
    public interface IStatusUpdate
    {
        /// <summary>
        /// 当在派生类中重写时, 处理 酷Q更新状态事件 回调
        /// </summary>
        /// <param name="sender">事件来源对象</param>
        /// <param name="e">附加的事件参数</param>
        /// <returns>返回用于展示的 <see cref="CQFloatWindow"/> 对象</returns>
        CQFloatWindow StatusUpdate(object sender, CQStatusUpdateEventArgs e);
    }
}
