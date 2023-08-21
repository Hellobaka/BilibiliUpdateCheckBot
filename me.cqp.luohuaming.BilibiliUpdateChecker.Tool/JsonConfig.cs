using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Tool
{
    /// <summary>
    /// 配置读取帮助类
    /// </summary>
    public static class JsonConfig
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string ConfigFileName = @"Config.json";
        private static object lockObject = new();
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="sectionName">需要读取的配置键名</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>目标类型的配置</returns>
        public static T GetConfig<T>(string sectionName, T defaultValue)
        {
            lock (lockObject)
            {
                if (File.Exists(ConfigFileName) is false)
                {
                    File.WriteAllText(ConfigFileName, "{}");
                }

                var o = JObject.Parse(File.ReadAllText(ConfigFileName));
                if (o.ContainsKey(sectionName))
                {
                    return o[sectionName]!.ToObject<T>();
                }

                if (defaultValue != null)
                {
                    WriteConfig<T>(sectionName, defaultValue);
                    return defaultValue;
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)"";
                }

                if (typeof(T) == typeof(int))
                {
                    return (T)(object)0;
                }

                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)false;
                }

                if (typeof(T) == typeof(object))
                {
                    return (T)(object)new { };
                }

                if (typeof(T) == typeof(int[]))
                {
                    return (T)(object)new int[] { };
                }

                if (typeof(T) == typeof(long[]))
                {
                    return (T)(object)new long[] { };
                }

                if (typeof(T) == typeof(List<int>))
                {
                    return (T)(object)new List<int> { };
                }

                if (typeof(T) == typeof(List<long>))
                {
                    return (T)(object)new List<long> { };
                }

                return typeof(T) == typeof(JObject) ? (T)(object)new JObject() : throw new Exception("无法默认返回");
            }
        }
        public static void WriteConfig<T>(string sectionName, T value)
        {
            lock (lockObject)
            {
                if (File.Exists(ConfigFileName) is false)
                {
                    File.WriteAllText(ConfigFileName, "{}");
                }

                var o = JObject.Parse(File.ReadAllText(ConfigFileName));
                if (o.ContainsKey(sectionName))
                {
                    o[sectionName] = JToken.FromObject(value);
                }
                else
                {
                    o.Add(sectionName, JToken.FromObject(value));
                }
                File.WriteAllText(ConfigFileName, o.ToString(Newtonsoft.Json.Formatting.Indented));
            }
        }
        public static void Init(string basePath)
        {
            ConfigFileName = Path.Combine(basePath, "Config.json");
            if (!File.Exists(ConfigFileName))
            {
                File.WriteAllText(ConfigFileName, "{}");
            }
        }
    }
}
