using System;

namespace FantasyFootballRobot.Core.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToFormatted(this TimeSpan ts)
        {
            return ts.ToString(@"hh\:mm\:ss\:fffffff");
        }
    }
}
