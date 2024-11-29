using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        // TODO: 移动配置至此类中
        // TODO: Mode改为枚举类型
        public AppConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static AppConfig Instance { get; private set; }

        public override void LoadConfig()
        {

        }
    }
}
