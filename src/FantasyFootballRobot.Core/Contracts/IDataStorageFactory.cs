namespace FantasyFootballRobot.Core.Contracts
{
   public interface IDataStorageFactory
   {
      IDataStorageSession CreateSession();
   }
}