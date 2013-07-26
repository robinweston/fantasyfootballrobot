using System;
using System.IO;

namespace FantasyFootballRobot.Core.Logging
{
    public class ImportantTextFileLogger : LoggerBase
    {
        private static string filePath;

        public ImportantTextFileLogger()
        {
        }

        public ImportantTextFileLogger(ILogger chainedLogger)
            : base(chainedLogger)
        {
            
        }

        public override void Log(Tag tag, string message, bool important = false)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                var fileName = string.Concat(DateTime.Now.ToString("dd-MM-yy HH-mm-ss"), " Important.txt");
                filePath = Path.Combine(@"log\", fileName);
            }

            if(important)
            {
                if (tag != PreviousTag)
                {
                    File.AppendAllText(filePath, Environment.NewLine);
                }
                var fullMessage = CreateMessageLine(tag, message);
                File.AppendAllText(filePath, fullMessage);
            }

            base.Log(tag, message, important);
        }

    }
}