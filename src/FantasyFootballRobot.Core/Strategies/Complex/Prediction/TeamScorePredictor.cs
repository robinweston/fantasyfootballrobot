using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Extensions;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;

namespace FantasyFootballRobot.Core.Strategies.Complex.Prediction
{
    public class TeamScorePredictor : ITeamScorePredictor
    {
        private readonly ITeamGameweekSelector _teamGameweekSelector;
        private readonly IPlayerScorePredictor _playerScorePredictor;
        private readonly ILogger _logger;

        private const int MaximumSensibleGameweekTeamScore = 200;

        public bool VerboseLoggingEnabled { get; set; }

        public TeamScorePredictor(ITeamGameweekSelector teamGameweekSelector, ILogger logger, IPlayerScorePredictor playerScorePredictor)
        {
            _teamGameweekSelector = teamGameweekSelector;
            _logger = logger;
            _playerScorePredictor = playerScorePredictor;
        }

        public double PredictTeamPointsForFutureGameweeks(Team team, int currentGameweek, int futureGameweeks, IList<Player> allPlayers, double futureGameweekMultiplier)
        {
            if(team == null)throw new ArgumentNullException("team");
            if (allPlayers == null) throw new ArgumentNullException("team");
            if (allPlayers.Count == 0) throw new ArgumentException("team");
            if (futureGameweekMultiplier > 1) throw new ArgumentOutOfRangeException("futureGameweekMultiplier");

            var cumulativePoints = 0.0;
            
            var maxGameweek = currentGameweek + futureGameweeks;   

            var gameweeksCalculated = 0;
            var gameweekMultiplier = 1.0;
            for (int gameweek = currentGameweek; gameweek < maxGameweek && gameweek <= 38; gameweek++)
            {
                var gameweekPoints = PredictTeamGameweekPoints(team, gameweek, allPlayers);

                // future gameweeks become less important the further into the future we go
                cumulativePoints += gameweekPoints * gameweekMultiplier;
                
                gameweeksCalculated++;
                gameweekMultiplier *= futureGameweekMultiplier;
            }

            var averagePointsPerGameweek = cumulativePoints / gameweeksCalculated;

            if (VerboseLoggingEnabled)
            {
                _logger.Log(Tag.Prediction, string.Format("Predicted team points for next {0} gameweeks: {1}", futureGameweeks, cumulativePoints.ToFormatted()));
                _logger.Log(Tag.Prediction, string.Format("Predicted team points per gameweek : {0}", averagePointsPerGameweek.ToFormatted()));
            }

            if (averagePointsPerGameweek > MaximumSensibleGameweekTeamScore)
            {
                throw new Exception(string.Format("Ëxpected to get {0} points per gameweek", averagePointsPerGameweek));
            }

            return cumulativePoints;
        }

        public virtual double PredictTeamGameweekPoints(Team team, int gameweek, IList<Player> allPlayers)
        {
            var playerScores = team.Players.Select(
                p => new PredictedPlayerScore{Player = p, PredictedScore = _playerScorePredictor.PredictPlayerGameweekPoints(p, gameweek, allPlayers)}).ToList();

            var teamSelection = _teamGameweekSelector.SelectStartingTeamForGameweek(playerScores);

            if (VerboseLoggingEnabled)
            {
                LogHelper.LogTeamSelection(teamSelection, gameweek, playerScores, _logger);
            }

            return teamSelection.PredictedTotalTeamScore;
        }
    }
}