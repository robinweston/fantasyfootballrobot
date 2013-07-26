namespace FantasyFootballRobot.Core.Entities.Utilities.Cloning
{
    public interface IDeepCloneable<out T>
    {
        T DeepClone();
    }
}