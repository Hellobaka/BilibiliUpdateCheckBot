using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliUpdateCheckBot.Models
{
    public class MonitorItem
    {
        public long GroupId { get; set; }

        public List<long> TargetId { get; set; }
    }
}
