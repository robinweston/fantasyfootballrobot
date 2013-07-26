using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public interface ITeamScorePredictor : ILoggable
    {
        double PredictTeamPointsForFutureGameweeks(Team team, int currentGameweek, int futureGameweeks, IList<Player> allPlayers, double futureGameweekMultiplier);
    }
}