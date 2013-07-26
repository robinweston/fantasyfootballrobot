namespace FantasyFootballRobot.Core.Entities.Utilities.Cloning
{
    public interface IShallowCloneable<out T>
    {
        T ShallowClone();
    }
}