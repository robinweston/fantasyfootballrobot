using System;
using System.IO;

namespace FantasyFootballRobot.Core.Logging
{
    public class CsvLogger : LoggerBase
    {
        private static string filePath;

        public CsvLogger()
        {
        }

        public CsvLogger(ILogger chainedLogger)
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

                var fileName = string.Concat(DateTime.Now.ToString("dd-MM-yy HH-mm-ss"), ".csv");
                filePath = Path.Combine(@"log\", fileName);
                File.AppendAllText(filePath, "Generation,Average Fitness,Top Fitness");
                File.AppendAllText(filePath, Environment.NewLine);
            }

            if (tag == Tag.CSV)
            {
                File.AppendAllText(filePath, message);
                File.AppendAllText(filePath, Environment.NewLine);
            }

            base.Log(tag, message, important);
        }
    }
}