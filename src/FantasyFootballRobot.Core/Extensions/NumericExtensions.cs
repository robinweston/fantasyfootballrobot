using System;

namespace FantasyFootballRobot.Core.Extensions
{
    public static class NumericExtensions
    {
        public static string ToMoney(this int money)
        {
            var floatValue = ((float)money) / 10;
            var moneyValue = floatValue.ToString("C");
            return string.Format("{0}M", moneyValue.Substring(0, moneyValue.Length - 1));
        }

        public static string ToFormatted(this int i)
        {
            return string.Format("{0:n0}", i);
        }

        public static string ToFormatted(this double d)
        {
            return string.Format("{0:N2}", d);
        }

        public static bool IsCloseToZero(this double d)
        {
            return Math.Abs(d - 0) < 0.001;
        }
    }
}
