using System;
using System.Runtime.InteropServices;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Sdk.Cqp.Core
{
    internal class Kernel32
    {
        [DllImport("kernel32.dll", EntryPoint = "lstrlenA", CharSet = CharSet.Ansi)]
        public static extern int LstrlenA(IntPtr ptr);
    }
}
