using System;
using System.IO;

namespace FantasyFootballRobot.Core.Logging
{
    public class TextFileLogger : LoggerBase
    {
        private static string filePath;

        private static object lockObj = new object();

        public TextFileLogger()
        {
        }

        public TextFileLogger(ILogger chainedLogger)
            : base(chainedLogger)
        {

        }

        public override void Log(Tag tag, string message, bool important = false)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                if (!Directory.Exists(@"log\"))
                {
                    Directory.CreateDirectory(@"log\");
                }

                var fileName = string.Concat(DateTime.Now.ToString("dd-MM-yy HH-mm-ss"), ".txt");
                filePath = Path.Combine(@"log\", fileName);
            }

            var fullMessage = CreateMessageLine(tag, message);

            lock (lockObj)
            {
                if (tag != PreviousTag)
                {
                    File.AppendAllText(filePath, Environment.NewLine);
                }
                File.AppendAllText(filePath, fullMessage);
            }

            base.Log(tag, message, important);
        }
    }
}