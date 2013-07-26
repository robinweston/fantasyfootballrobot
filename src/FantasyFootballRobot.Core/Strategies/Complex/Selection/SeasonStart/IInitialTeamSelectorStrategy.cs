using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart
{
    public interface IInitialTeamSelectorStrategy
    {
        Team SelectTeam(IList<Player> allPlayers);
    }
}