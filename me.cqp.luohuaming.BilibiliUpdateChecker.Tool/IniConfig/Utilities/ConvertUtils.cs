using System;
using System.Globalization;
using System.Numerics;

namespace me.cqp.luohuaming.BilibiliUpdateChecker.Tool.IniConfig.Utilities
{
    internal static class ConvertUtils
    {
        internal static TimeSpan StringToTimeSpan(string input)
        {
            return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
        }

        internal static BigInteger ToBigInteger(object value)
        {
            if (value is BigInteger)
            {
                return (BigInteger)value;
            }

            if (value is string value2)
            {
                return BigInteger.Parse(value2, CultureInfo.InvariantCulture);
            }
            if (value is float value3)
            {
                return new BigInteger(value3);
            }
            if (value is double value4)
            {
                return new BigInteger(value4);
            }
            if (value is decimal value5)
            {
                return new BigInteger(value5);
            }
            if (value is int value6)
            {
                return new BigInteger(value6);
            }
            return value is long value7
                ? new BigInteger(value7)
                : value is uint value8
                ? new BigInteger(value8)
                : value is ulong value9
                ? new BigInteger(value9)
                : value is byte[] value10
                ? new BigInteger(value10)
                :
            throw new InvalidCastException(string.Format("无法将 {0} 转换为 BigInteger.", value.GetType().Name));
        }
    }
}
