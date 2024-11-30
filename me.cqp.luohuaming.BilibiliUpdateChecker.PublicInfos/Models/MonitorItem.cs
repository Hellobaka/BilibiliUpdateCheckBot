using System.Collections.Generic;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos.Models
{
    public class MonitorItem
    {
        public long GroupId { get; set; }

        public List<long> TargetId { get; set; }
    }
}
