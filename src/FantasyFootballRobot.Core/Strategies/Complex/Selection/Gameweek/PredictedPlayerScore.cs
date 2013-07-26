using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek
{
    public class PredictedPlayerScore
    {
        public Player Player { get; set; }
        public double PredictedScore { get; set; }
    }
}