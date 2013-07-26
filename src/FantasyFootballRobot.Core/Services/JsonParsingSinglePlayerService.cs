using System;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Entities.Json;
using FantasyFootballRobot.Core.Logging;
using Newtonsoft.Json;

namespace FantasyFootballRobot.Core.Services
{
    public class JsonParsingSinglePlayerService : ISinglePlayerService
    {
        private readonly IPlayerJsonService _localPlayerJsonService;
        private readonly ILogger _logger;


        public JsonParsingSinglePlayerService(IPlayerJsonService localPlayerJsonService, ILogger logger)
        {
            _localPlayerJsonService = localPlayerJsonService;
            _logger = logger;
        }

        public Player GetPlayer(int playerId)
        {
            var json = _localPlayerJsonService.GetPlayerJson(playerId);
                    
            Player player = null;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    player = HydratePlayerFromJson(json);
                }
                catch (JsonReaderException)
                {
                    _logger.Log(Tag.PlayerService, string.Format("Player {0} has invalid JSON. Could be legitimate if last player", playerId));
                }
            }
            return player;
        }

        static Player HydratePlayerFromJson(string json)
        {
            
            var jsonPlayer = new JsonPlayer(json);
            return AutoMapper.Mapper.Map<JsonPlayer, Player>(jsonPlayer);
        }
    }
}