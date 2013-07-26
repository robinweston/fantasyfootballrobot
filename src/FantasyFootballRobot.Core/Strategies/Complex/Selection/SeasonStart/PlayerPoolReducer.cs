using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;

namespace FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart
{
    public class PlayerPoolReducer : IPlayerPoolReducer
    {
        private readonly ILogger _logger;
        private readonly IConfigurationSettings _configSettings;
        private readonly IInitialTeamSelectionParameters _teamSelectionParameters;

        public PlayerPoolReducer(ILogger logger, IInitialTeamSelectionParameters teamSelectionParameters, IConfigurationSettings configSettings)
        {
            _logger = logger;
            _teamSelectionParameters = teamSelectionParameters;
            _configSettings = configSettings;
        }

        public IList<Player> ReducePlayerPool(IList<Player> allPlayers)
        {
            List<Player> reducedPlayerPool = allPlayers.Where(ShouldAddPlayerToPool).ToList();

            _logger.Log(Tag.Genetic, string.Format("Reduced seed players to {0}", reducedPlayerPool.Count));
            return reducedPlayerPool;
        }

        private bool ShouldAddPlayerToPool(Player player)
        {
            var playerHasPlayedThisSeason = player.PastFixtures.Sum(pf => pf.MinutesPlayed) > 0;

            return playerHasPlayedThisSeason || DidPlayerPerformWellEnoughLastSeason(player);

        }

        private bool DidPlayerPerformWellEnoughLastSeason(Player player)
        {
            var lastSeason = player.GetPastSeason(_configSettings.SeasonStartYear);

            return lastSeason != null &&
                   lastSeason.TotalPointsScored >=
                   _teamSelectionParameters.MinimumPlayerScoreFromPreviousSeasonToBeConsidered &&
                   lastSeason.MinutesPlayed >=
                   _teamSelectionParameters.MinimumPlayerMinutesPlayerFromPreviousSeasonToBeConsidered;
        }
    }
}