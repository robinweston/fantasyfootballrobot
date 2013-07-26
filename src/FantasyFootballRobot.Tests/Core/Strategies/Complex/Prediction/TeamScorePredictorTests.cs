using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Tests.Helpers;
using Moq;
using NUnit.Framework;

namespace FantasyFootballRobot.Tests.Core.Strategies.Complex.Prediction
{
    [TestFixture]
    public class TeamScorePredictorTests
    {
        private Mock<TeamScorePredictor> _teamScorePredictorMock;
        private Team _team;
        private const double FutureGameweekScoreMultiplier = 1.0;

        private Mock<ITeamGameweekSelector> _teamSelectorMock;
        private Mock<IPlayerScorePredictor> _playerScorePredictorMock;
        private TeamSelection _teamSelection;
        private int _gameweek;
        private IList<Player> _allPlayers;

        [SetUp]
        public void SetUp()
        {
            _gameweek = 1;

            _team = TeamCreationHelper.CreateTestTeam();

            _allPlayers = TeamCreationHelper.CreatePlayerList(10, 10, 10, 10);

            _teamSelectorMock = new Mock<ITeamGameweekSelector>();
            _teamSelection = CreateSelectorResult();
            _teamSelectorMock.Setup(x => x.SelectStartingTeamForGameweek(It.IsAny<IList<PredictedPlayerScore>>())).Returns(_teamSelection);

            _playerScorePredictorMock = new Mock<IPlayerScorePredictor>();

            _teamScorePredictorMock = new Mock<TeamScorePredictor>(_teamSelectorMock.Object, new Mock<ILogger>().Object, _playerScorePredictorMock.Object){CallBase = true};
        }


        [Test]
        public void team_predictor_adds_up_predictions_for_each_gameweek()
        {
            // Arrange
            _teamScorePredictorMock.Setup(x => x.PredictTeamGameweekPoints(_team, It.IsAny<int>(), It.IsAny<IList<Player>>())).Returns(
                (Team team, int gameweek, IList<Player> allPlayers) => gameweek);

            // Act 
            var predictedScore = _teamScorePredictorMock.Object.PredictTeamPointsForFutureGameweeks(_team, 1, 5, _allPlayers, FutureGameweekScoreMultiplier);

            // Assert
            Assert.That(predictedScore, Is.EqualTo(1 + 2 + 3 + 4 + 5));
        }

        [Test]
        public void team_predictor_runs_predictions_for_correct_future_gameweeks_if_mid_season()
        {
            // Arrange
            const int futureGameweeks = 5;
            const int currentGameweek = 3;

            // Act 
            _teamScorePredictorMock.Object.PredictTeamPointsForFutureGameweeks(_team, currentGameweek, futureGameweeks, _allPlayers, FutureGameweekScoreMultiplier);

            // Assert
            for(var i =0; i < futureGameweeks; i++)
            {
                var gameweek = currentGameweek + i;
                _teamScorePredictorMock.Verify(x => x.PredictTeamGameweekPoints(_team, gameweek, _allPlayers), Times.Once());
            }
            
        }

        [Test]
        public void team_predictor_runs_predictions_for_correct_future_gameweeks_if_at_season_end()
        {
            // Arrange
            const int futureGameweeks = 5;
            const int currentGameweek = 36;

            // Act 
            _teamScorePredictorMock.Object.PredictTeamPointsForFutureGameweeks(_team, currentGameweek, futureGameweeks, _allPlayers, FutureGameweekScoreMultiplier);

            // Assert
            _teamScorePredictorMock.Verify(x => x.PredictTeamGameweekPoints(_team, 36, _allPlayers), Times.Once());
            _teamScorePredictorMock.Verify(x => x.PredictTeamGameweekPoints(_team, 37, _allPlayers), Times.Once());
            _teamScorePredictorMock.Verify(x => x.PredictTeamGameweekPoints(_team, 38, _allPlayers), Times.Once());
            _teamScorePredictorMock.Verify(x => x.PredictTeamGameweekPoints(_team, 39, _allPlayers), Times.Never());
        }

        [Test]
        public void gameweek_predictor_predicts_score_of_each_individual_player()
        {
            // Arrange
            _playerScorePredictorMock.Setup(x => x.PredictPlayerGameweekPoints(It.IsAny<Player>(), It.IsAny<int>(), It.IsAny<IList<Player>>()));

            // Act
            _teamScorePredictorMock.Object.PredictTeamGameweekPoints(_team, _gameweek, _allPlayers);

            // Assert
            foreach(var player in _team.Players)
            {
                Player player1 = player;
                _playerScorePredictorMock.Verify(x => x.PredictPlayerGameweekPoints(player1, _gameweek, _allPlayers));
            }
        }
            
        [Test]
        public void predictor_selects_starting_team_for_gameweek()
        {
            // Arrange
            //predict each player to score the same number of points as their id
            _playerScorePredictorMock.Setup(x => x.PredictPlayerGameweekPoints(It.IsAny<Player>(), It.IsAny<int>(), It.IsAny<IList<Player>>())).Returns((Player player, int gameweek, IList<Player> allPlayers) => player.Id);

            // Act
            _teamScorePredictorMock.Object.PredictTeamGameweekPoints(_team, _gameweek, _allPlayers);

            // Assert

            //check that each player has a score equal to their id
            _teamSelectorMock.Verify(x => x.SelectStartingTeamForGameweek(It.Is<IList<PredictedPlayerScore>>(scores => scores.All(p => p.Player.Id == (int)p.PredictedScore))));
        }

        [Test]
        public void predicted_gameweek_performance_returns_points_total_from_selector()
        {
            // Arrange
            const int predictedTeamScore = 50;
            _teamSelection.PredictedTotalTeamScore = predictedTeamScore;
     
            // Act
            var prediction = _teamScorePredictorMock.Object.PredictTeamGameweekPoints(_team, _gameweek, _allPlayers);

            // Assert
            Assert.That(prediction, Is.EqualTo(predictedTeamScore));

        }

        [Test]
        public void team_scores_decrease_the_more_into_the_future_they_get()
        {
            // Arrange
            //100 for each gameweek as a team score
            _teamSelectorMock.Setup(x => x.SelectStartingTeamForGameweek(It.IsAny<IList<PredictedPlayerScore>>())).Returns(new TeamSelection{PredictedTotalTeamScore = 100});

            const int futureGameweeks = 3;
            const int currentGameweek = 1;
            const double futureGameweekMultiplier = 0.5;

            // Act
            var score = _teamScorePredictorMock.Object.PredictTeamPointsForFutureGameweeks(_team, currentGameweek, futureGameweeks, _allPlayers, futureGameweekMultiplier);

            // Assert    
            Assert.That(score, Is.EqualTo(100 + (100 * 0.5) + (100 * 0.5 * 0.5)));           
        }


        private static TeamSelection CreateSelectorResult()
        {
            var teamSelectorResult = new TeamSelection
            {
                Team = TeamCreationHelper.CreateTestTeam()
            };

            return teamSelectorResult;
        }
 
    }
}
