namespace FantasyFootballRobot.Core.Logging
{
    public interface ILogger
    {
        void Log(Tag tag, string message, bool important = false);
    }
}
