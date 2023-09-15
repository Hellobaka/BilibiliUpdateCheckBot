using ChromeCookieDecrypt_Framework;
using me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Model;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool;
using me.cqp.luohuaming.BilibiliUpdateChecker.Tool.IniConfig;
using System;
using System.IO;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.PublicInfos
{
    public static class CommonHelper
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>
        /// 获取CQ码中的图片网址
        /// </summary>
        /// <param name="imageCQCode">需要解析的图片CQ码</param>
        /// <returns></returns>
        public static string GetImageURL(string imageCQCode)
        {
            string path = MainSave.ImageDirectory + CQCode.Parse(imageCQCode)[0].Items["file"] + ".cqimg";
            IniConfig image = new IniConfig(path);
            image.Load();
            return image.Object["image"]["url"].ToString();
        }

        public static string GetAppImageDirectory()
        {
            var ImageDirectory = Path.Combine(Environment.CurrentDirectory, "data", "image\\");
            return ImageDirectory;
        }

        public static void UpdateCookie()
        {
            try
            {
                string cookie = "";
                var cookies = ChromeCookieDecrypt.QueryCookies(".bilibili.com");
                foreach (var item in cookies)
                {
                    cookie += $"{item.Key}={item.Value};";
                }
                JsonConfig.WriteConfig("Cookies", cookie);
                BilibiliMonitor.UpdateChecker.Instance.Cookies = cookie;
            }
            catch (Exception ex)
            {
                MainSave.CQLog.Error("UpdateCookie", ex.Message + ex.StackTrace);
            }
        }
    }
}