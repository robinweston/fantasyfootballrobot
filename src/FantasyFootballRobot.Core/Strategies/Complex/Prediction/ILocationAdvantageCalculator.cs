using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public interface ILocationAdvantageCalculator
    {
        HomeAdvantageResult CalculateLocationAdvantage(IList<Player> players);
    }
}
