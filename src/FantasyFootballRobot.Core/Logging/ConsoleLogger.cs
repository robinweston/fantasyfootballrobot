using System;

namespace FantasyFootballRobot.Core.Logging
{
    public class ConsoleLogger : LoggerBase
    {
        public ConsoleLogger(ILogger chainedLogger) : base(chainedLogger)
        {          
        }

        public override void Log(Tag tag, string message, bool important = false)
        {
            SetConsoleColor(tag);

            var fullMessage = CreateMessageLine(tag, message);

            if(tag != PreviousTag)
            {
                Console.WriteLine();
            }

            Console.Write(fullMessage);

            Console.ResetColor();

            base.Log(tag, message, important);
        }

        private static void SetConsoleColor(Tag tag)
        {
            var consoleColor = ConsoleColor.White;
            switch(tag)
            {
                case Tag.Error:
                    consoleColor = ConsoleColor.Red;
                    break;
                case Tag.Genetic:
                    consoleColor = ConsoleColor.Green;
                    break;
                case Tag.Progress:
                    consoleColor = ConsoleColor.DarkCyan;
                    break;
                case Tag.Prediction:
                    consoleColor = ConsoleColor.DarkMagenta;
                    break;
            }

            Console.ForegroundColor = consoleColor;
        }
    }
}