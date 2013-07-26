using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;

namespace FantasyFootballRobot.Core.Strategies.Complex
{
    public class ComplexStrategy : IStrategy
    {
        readonly ILogger _logger;
        readonly IInitialTeamSelectorStrategy _initialTeamSelectorStrategy;
        private readonly IPlayerScorePredictor _playerScorePredictor;
        private readonly ITeamGameweekSelector _teamGameweekSelector;
        private readonly ITransferSelectorStrategy _transferSelectorStrategy; 

        public ComplexStrategy(ILogger logger, IInitialTeamSelectorStrategy initialTeamSelectorStrategy, IPlayerScorePredictor playerScorePredictor, ITeamGameweekSelector teamGameweekSelector, ITransferSelectorStrategy transferSelectorStrategy)
        {
            _logger = logger;
            _initialTeamSelectorStrategy = initialTeamSelectorStrategy;
            _playerScorePredictor = playerScorePredictor;

            _teamGameweekSelector = teamGameweekSelector;
            _transferSelectorStrategy = transferSelectorStrategy;

            _logger.Log(Tag.Strategy, "Using Complex strategy");
        }

        public TransferActions MakeTransfers(SeasonState seasonState)
        {
            return _transferSelectorStrategy.SelectTransfers(seasonState);
        }

        public Team PickGameweekTeam(SeasonState seasonState)
        {
            var predictedPlayerScores =
                seasonState.CurrentTeam.Players.Select(
                    p => new PredictedPlayerScore{Player = p, PredictedScore = 
                    _playerScorePredictor.PredictPlayerGameweekPoints(p, seasonState.Gameweek, seasonState.AllPlayers)}).ToList();

            var teamSelection = _teamGameweekSelector.SelectStartingTeamForGameweek(predictedPlayerScores);
            
            LogHelper.LogTeamSelection(teamSelection, seasonState.Gameweek, predictedPlayerScores, _logger, true);

            return teamSelection.Team;
        }


        public Team PickStartingTeam(IList<Player> allPlayers)
        {
            return _initialTeamSelectorStrategy.SelectTeam(allPlayers);
        }
    }
}
