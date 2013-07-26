using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart
{
    public interface IPlayerPoolReducer
    {
        IList<Player> ReducePlayerPool(IList<Player> allPlayers);
    }
}
