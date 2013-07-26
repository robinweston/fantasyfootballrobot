using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Simulators;

namespace FantasyFootballRobot.Core.Strategies
{
    public interface IStrategy
    {
        TransferActions MakeTransfers(SeasonState seasonState);
        Team PickGameweekTeam(SeasonState seasonState);
        Team PickStartingTeam(IList<Player> allPlayers);
    }
}