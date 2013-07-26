using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Simulators
{
    public interface ITimeAdjustor
    {
        IList<Player> AdjustPlayersToGameweek(IList<Player> allUpToDatePlayers, int gameweek);
        Team AdjustTeamToGameweek(Team team, IList<Player> allUpToDatePlayers, int gameweek);
    }
}
