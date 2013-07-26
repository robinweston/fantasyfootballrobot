namespace FantasyFootballRobot.Core.Caching
{
    public interface ICacher
    {
        int CacheHits { get; }
        int CacheMisses { get; }
    }
}