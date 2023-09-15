using System;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Tool.IniConfig.Utilities
{
    internal class ReflectionUtils
    {
        internal static bool IsNullableType(Type t)
        {
            return t == null ? throw new ArgumentNullException("t") : t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
