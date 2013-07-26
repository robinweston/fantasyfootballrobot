namespace FantasyFootballRobot.Core.Strategies
{
    public interface IRandom
    {
        int Next(int maxValue);
        double NextDouble();
    }
}
