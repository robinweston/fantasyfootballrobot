using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Simulators
{
    public interface IGameweekSimulator
    {
        IList<PlayerGameweekPerformance> CalculatePlayerPerformances(Team team, int gameweek, IList<Player> allUpToDatePlayers);
    }
}
