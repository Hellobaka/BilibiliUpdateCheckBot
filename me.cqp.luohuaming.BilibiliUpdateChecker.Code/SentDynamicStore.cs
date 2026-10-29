using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Code
{
    internal sealed class SentDynamicStore
    {
        private readonly string filePath;
        private readonly object syncRoot = new();
        private readonly Dictionary<long, HashSet<string>> sent;
        private bool dirty;

        public SentDynamicStore(string filePath)
        {
            this.filePath = Path.GetFullPath(filePath);
            sent = File.Exists(this.filePath)
                ? JsonConvert.DeserializeObject<Dictionary<long, HashSet<string>>>(File.ReadAllText(this.filePath))
                    ?? throw new InvalidDataException("已发送动态记录不能为空")
                : new Dictionary<long, HashSet<string>>();
            if (sent.Values.Any(ids => ids == null))
            {
                throw new InvalidDataException("已发送动态记录格式错误");
            }
        }

        public void SendOnce(long groupId, string dynamicId, Func<bool> send)
        {
            lock (syncRoot)
            {
                if (!sent.TryGetValue(groupId, out var ids) || !ids.Contains(dynamicId))
                {
                    if (!send())
                    {
                        throw new InvalidOperationException($"动态 {dynamicId} 发送到群 {groupId} 失败");
                    }

                    if (ids == null)
                    {
                        ids = new HashSet<string>();
                        sent.Add(groupId, ids);
                    }
                    ids.Add(dynamicId);
                    dirty = true;
                }

                // 保存失败时保留内存记录，下次重试只补写文件，避免再次发送。
                if (dirty)
                {
                    Save();
                    dirty = false;
                }
            }
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string temporaryPath = filePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(sent, Formatting.Indented));
            if (File.Exists(filePath))
            {
                File.Replace(temporaryPath, filePath, null);
            }
            else
            {
                File.Move(temporaryPath, filePath);
            }
        }
    }
}
