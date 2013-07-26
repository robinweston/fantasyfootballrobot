using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public interface IPlayerScorePredictor : ILoggable
    {
        double PredictPlayerGameweekPoints(Player player, int gameweek, IList<Player> allPlayers);
    }
}