using System;

namespace FantasyFootballRobot.Core.Exceptions
{
    public class TimeAdjustorException : Exception
    {
        public TimeAdjustorException(string message) : base(message)
        {
        }
    }
}