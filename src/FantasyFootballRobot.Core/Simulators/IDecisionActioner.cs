using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Strategies;

namespace FantasyFootballRobot.Core.Simulators
{
    public interface IDecisionActioner
    {
        TransferActionsResult ValidateAndApplyTransfers(SeasonState seasonState, TransferActions transferActions);
        SeasonState ValidateAndApplyGameweekTeamSelection(SeasonState seasonState, Team selectedTeam);
        SeasonState ValidateAndApplyStartingTeam(Team startingTeam, IList<Player> allPlayers);
    }
}