using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Services
{
    public interface ISinglePlayerService
    {
        Player GetPlayer(int playerId);
    }
}
