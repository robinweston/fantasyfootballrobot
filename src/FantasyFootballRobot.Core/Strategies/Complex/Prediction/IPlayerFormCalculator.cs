using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public interface IPlayerFormCalculator
    {
        PlayerForm CalculateCurrentPlayerForm(Player player, IList<Player> allPlayers);
    }
}