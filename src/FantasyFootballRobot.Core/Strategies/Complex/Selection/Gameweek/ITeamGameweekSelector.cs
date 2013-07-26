using System.Collections.Generic;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek
{
    public interface ITeamGameweekSelector
    {
        TeamSelection SelectStartingTeamForGameweek(IList<PredictedPlayerScore> predictedPlayerScores);
    }
}