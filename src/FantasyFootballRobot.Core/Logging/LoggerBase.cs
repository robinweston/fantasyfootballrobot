using System;

namespace FantasyFootballRobot.Core.Logging
{
    public abstract class LoggerBase : ILogger
    {
        private readonly ILogger _chainedLogger;
        protected Tag PreviousTag { get; set; }

        protected LoggerBase(ILogger chainedLogger)
        {
            _chainedLogger = chainedLogger;
        }

        protected LoggerBase()
        {
        }

        public virtual void Log(Tag tag, string message, bool important = false)
        {
            PreviousTag = tag;

            if(_chainedLogger != null)
            {
                _chainedLogger.Log(tag, message, important);
            }
        }

        protected string CreateMessageLine(Tag tag, string message)
        {
            var timeString = DateTime.Now.ToString("HH:mm:ss");
            return string.Concat(timeString, " ", tag, " - ", message, Environment.NewLine);
        }
    }
}
