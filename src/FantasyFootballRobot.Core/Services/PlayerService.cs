using System.Collections.Generic;
using FantasyFootballRobot.Core.Entities;

namespace FantasyFootballRobot.Core.Services
{
    public class PlayerService : IMultiplePlayersService
    {
        private readonly ISinglePlayerService _childSinglePlayerService;
        private IList<Player> _cachedPlayerList;

        public PlayerService(ISinglePlayerService childSinglePlayerService)
        {
            _childSinglePlayerService = childSinglePlayerService;
        }

        public IList<Player> GetAllPlayers()
        {
            return _cachedPlayerList ?? (_cachedPlayerList = RetrievePlayersFromChildService());
        }

        private IList<Player> RetrievePlayersFromChildService()
        {
            var players = new List<Player>();
            Player player;
            int id = 1;
            do
            {
                player = GetPlayer(id);

                if (player != null)
                {
                    players.Add(player);
                }

                id++;
            } while (player != null);

            return players;
        }

        private Player GetPlayer(int playerId)
        {
            return _childSinglePlayerService.GetPlayer(playerId);
        }
    }
}