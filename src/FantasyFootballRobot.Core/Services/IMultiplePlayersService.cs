using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Services
{
    public interface IMultiplePlayersService
    {
        IList<Player> GetAllPlayers();
    }
}