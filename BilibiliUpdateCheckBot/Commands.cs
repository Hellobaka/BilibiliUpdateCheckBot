using Another_Mirai_Native.Abstractions;
using Another_Mirai_Native.Abstractions.Attributes;
using Another_Mirai_Native.Abstractions.Context;
using Another_Mirai_Native.Abstractions.Enums;
using Another_Mirai_Native.Abstractions.Handlers;
using BilibiliMonitor.BilibiliAPI;
using BilibiliUpdateCheckBot.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BilibiliUpdateCheckBot
{
    public class Commands : CommandHandlerBase
    {
        public string AddBangumi { get; set; } = "^#ABCD (?<sid>\\d*)$";
        public string RemoveBangumi { get; set; } = "^#ABCD (?<sid>\\d*)$";
        public string AddDynamic { get; set; } = "^#ABCD (?<uid>\\d*)$";
        public string RemoveDynamic { get; set; } = "^#ABCD (?<uid>\\d*)$";
        public string AddStream { get; set; } = "^#ABCD (?<uid>\\d*)$";
        public string RemoveStream { get; set; } = "^#ABCD (?<uid>\\d*)$";
        public string BangumiList { get; set; } = "#ABCD";
        public string DynamicList { get; set; } = "#ABCD";
        public string StreamList { get; set; } = "#ABCD";

        [DynamicCommand(nameof(AddBangumi), MatchMode.Regex)]
        public async Task AddBangumiCommand(GroupMessageContext e, long sid)
        {
            var bangumisList = Entry.Config.Bangumis;
            var group = Entry.Config.MonitorBangumis;
            Bangumi? ban = null;
            if (!bangumisList.Any(x => x == sid))
            {
                bangumisList.Add(sid);
                ban = Bangumi.AddBangumi(sid);
            }
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
            if (groupItem != null)
            {
                if (groupItem.TargetId.Contains(sid) && bangumisList.Any(x => x == e.FromGroup.Id))
                {
                    await e.FromGroup.SendGroupMessageAsync("重复添加");
                    return;
                }
                groupItem.TargetId.Add(sid);
            }
            else
            {
                group.Add(new MonitorItem { GroupId = e.FromGroup.Id, TargetId = [sid] });
            }
            if (ban != null)
            {
                Entry.Config.Bangumis = bangumisList;
                Entry.Config.MonitorBangumis = group;
                Entry.Config.SaveConfig();

                await e.FromGroup.SendGroupMessageAsync($"{ban.Name} 添加番剧更新检查成功");
            }
            else
            {
                await e.FromGroup.SendGroupMessageAsync("添加失败，可能番剧不存在或非支持地区番剧");
            }
        }

        [DynamicCommand(nameof(AddDynamic), MatchMode.Regex)]
        public async Task AddDynamicCommand(GroupMessageContext e, long uid)
        {
            var dynamicsList = Entry.Config.Dynamics;
            var group = Entry.Config.MonitorDynamics;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
            if (groupItem != null)
            {
                if (groupItem.TargetId.Contains(uid) && dynamicsList.Any(x => x == e.FromGroup.Id))
                {
                    await e.FromGroup.SendGroupMessageAsync("重复添加");
                    return;
                }

                groupItem.TargetId.Add(uid);
            }
            else
            {
                group.Add(new MonitorItem { GroupId = e.FromGroup.Id, TargetId = [uid] });
            }
            Entry.Config.MonitorDynamics = group;
            if (!dynamicsList.Any(x => x == uid))
            {
                dynamicsList.Add(uid);
                var dy = Dynamics.AddDynamic(uid);
                Entry.Config.Dynamics = dynamicsList;
            }
            Entry.Config.SaveConfig();
            var c = Dynamics.GetDynamic(uid);
            Entry.Config.SaveConfig();
            if (c != null)
            {
                await e.FromGroup.SendGroupMessageAsync($"{c.UserName} 添加动态监视成功");
            }
            else
            {
                await e.FromGroup.SendGroupMessageAsync("添加失败");
            }
        }

        [DynamicCommand(nameof(AddStream), MatchMode.Regex)]
        public async Task AddStreamCommand(GroupMessageContext e, long uid)
        {
            var streams = Entry.Config.Streams;
            var group = Entry.Config.MonitorStreams;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
            if (groupItem != null)
            {
                if (groupItem.TargetId.Contains(uid) && streams.Any(x => x == e.FromGroup.Id))
                {
                    await e.FromGroup.SendGroupMessageAsync("重复添加");
                    return;
                }

                groupItem.TargetId.Add(uid);
            }
            else
            {
                group.Add(new MonitorItem { GroupId = e.FromGroup.Id, TargetId = [uid] });
            }
            Entry.Config.MonitorStreams = group;
            LiveStreams live = null;
            if (!streams.Any(x => x == uid))
            {
                streams.Add(uid);
                live = LiveStreams.AddStream(uid);
                Entry.Config.Streams = streams;
            }
            Entry.Config.SaveConfig();
            if (live != null)
            {
                await e.FromGroup.SendGroupMessageAsync($"{live.Name} 添加直播监视成功");
            }
            else
            {
                await e.FromGroup.SendGroupMessageAsync("添加失败");
            }
        }

        [DynamicCommand(nameof(BangumiList), MatchMode.FullMatch)]
        public async Task BangumiListCommand(GroupMessageContext e)
        {
            StringBuilder sb = new();
            sb.AppendLine("番剧列表");
            int index = 1;
            foreach (var item in Bangumi.GetBangumiList())
            {
                var group = Entry.Config.MonitorBangumis;
                var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
                if (groupItem != null && groupItem.TargetId.Any(x => x == item.Item1))
                {
                    sb.AppendLine($"{index}. {item.Item2} - {item.Item1}");
                    index++;
                }
            }
            await e.FromGroup.SendGroupMessageAsync(sb.ToString());
        }

        [DynamicCommand(nameof(DynamicList), MatchMode.FullMatch)]
        public async Task DynamicListCommand(GroupMessageContext e)
        {
            StringBuilder sb = new();
            sb.AppendLine("动态列表");
            int index = 1;
            foreach (var item in Dynamics.GetDynamicList())
            {
                var group = Entry.Config.MonitorDynamics;
                var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
                if (groupItem != null && groupItem.TargetId.Any(x => x == item.Item1))
                {
                    sb.AppendLine($"{index}. {item.Item2} - {item.Item1}");
                    index++;
                }
            }
            await e.FromGroup.SendGroupMessageAsync(sb.ToString());
        }
        [DynamicCommand(nameof(StreamList), MatchMode.FullMatch)]
        public async Task StreamListCommand(GroupMessageContext e)
        {
            StringBuilder sb = new();
            sb.AppendLine("直播列表");
            int index = 1;
            foreach (var item in LiveStreams.GetStreamList())
            {
                var group = Entry.Config.MonitorStreams;
                var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
                if (groupItem != null && groupItem.TargetId.Any(x => x == item.Item1))
                {
                    sb.AppendLine($"{index}. {item.Item2} - {item.Item1}{(item.Item3 ? "[直播中]" : "")}");
                    index++;
                }
            }
            await e.FromGroup.SendGroupMessageAsync(sb.ToString());
        }

        [DynamicCommand(nameof(RemoveBangumi), MatchMode.Regex)]
        public async Task RemoveBangumiCommand(GroupMessageContext e, long sid)
        {
            var bangumis = Entry.Config.Bangumis;
            var group = Entry.Config.MonitorBangumis;

            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
            if (groupItem != null)
            {
                if (!groupItem.TargetId.Contains(sid))
                {
                    if (groupItem.TargetId.Count >= sid)
                    {
                        sid = groupItem.TargetId[(int)(sid - 1)];
                    }
                    else
                    {
                        await e.FromGroup.SendGroupMessageAsync("番剧sid或序号格式不正确");
                        return;
                    }
                }
                groupItem.TargetId.Remove(sid);
            }
            Entry.Config.MonitorBangumis = group;
            bool existFlag = false;
            foreach (var item in group)
            {
                if (item.TargetId.Contains(sid))
                {
                    existFlag = true;
                    break;
                }
            }
            if (bangumis.Any(x => x == sid) && !existFlag)
            {
                bangumis.Remove(sid);
                Bangumi.RemoveBangumi(sid);
                Entry.Config.Bangumis = bangumis;
            }
            Entry.Config.SaveConfig();
            await e.FromGroup.SendGroupMessageAsync("删除成功");
        }
        [DynamicCommand(nameof(RemoveDynamic), MatchMode.Regex)]
        public async Task RemoveDynamicCommand(GroupMessageContext e, long uid)
        {
            var dynamics = Entry.Config.Dynamics;
            var group = Entry.Config.MonitorDynamics;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
            if (groupItem != null)
            {
                if (!groupItem.TargetId.Contains(uid))
                {
                    if (groupItem.TargetId.Count > uid)
                    {
                        uid = groupItem.TargetId[(int)uid - 1];
                    }
                    else
                    {
                        await e.FromGroup.SendGroupMessageAsync("用户ID或序号格式不正确");
                        return;
                    }
                }

                groupItem.TargetId.Remove(uid);
            }
            Entry.Config.MonitorDynamics = group;
            bool existFlag = false;
            foreach (var item in group)
            {
                if (item.TargetId.Contains(uid))
                {
                    existFlag = true;
                    break;
                }
            }
            if (dynamics.Any(x => x == uid) && !existFlag)
            {
                dynamics.Remove(uid);
                Dynamics.RemoveDynamic(uid);
                Entry.Config.Dynamics = dynamics;
            }

            Entry.Config.SaveConfig();
            await e.FromGroup.SendGroupMessageAsync("删除成功");
        }
        [DynamicCommand(nameof(RemoveStream), MatchMode.Regex)]
        public async Task RemoveStreamCommand(GroupMessageContext e, long uid)
        {
            var streams = Entry.Config.Streams;
            var group = Entry.Config.MonitorStreams;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup.Id);
            if (groupItem != null)
            {
                if (!groupItem.TargetId.Contains(uid))
                {
                    if (groupItem.TargetId.Count > uid)
                    {
                        uid = groupItem.TargetId[(int)uid - 1];
                    }
                    else
                    {
                        await e.FromGroup.SendGroupMessageAsync("用户ID或序号格式不正确");
                        return;
                    }
                }

                groupItem.TargetId.Remove(uid);
            }
            Entry.Config.MonitorStreams = group;
            bool existFlag = false;
            foreach (var item in group)
            {
                if (item.TargetId.Contains(uid))
                {
                    existFlag = true;
                    break;
                }
            }
            if (streams.Any(x => x == uid) && !existFlag)
            {
                streams.Remove(uid);
                LiveStreams.RemoveStream(uid);
                Entry.Config.Streams = streams;
            }

            Entry.Config.SaveConfig();
            await e.FromGroup.SendGroupMessageAsync("删除成功");
        }

        [Command(MatchMode.Contain, "")]
        public async Task VideoQuery()
        {

        }
    }
}
