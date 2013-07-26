using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public interface ITeamStrengthCalculator
    {
        /// <summary>
        /// 1.0 is average team strength
        /// </summary>
        /// <returns></returns>
        TeamStrength CalculateTeamStrength(string clubCode, IList<Player> allPlayers);
    }
}
